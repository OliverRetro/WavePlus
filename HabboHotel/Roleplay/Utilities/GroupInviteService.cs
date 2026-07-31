using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Overlay;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Users;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Plus.HabboHotel.Roleplay.Utilities
{
    public static class GroupInviteService
    {
        public const int ExpirySeconds = 60;

        public class PendingInvite
        {
            public int Id { get; set; }
            public int GroupId { get; set; }
            public int SenderId { get; set; }
            public string SenderName { get; set; }
            public int TargetId { get; set; }
            public DateTime SentAt { get; set; }

            public bool IsExpired => (DateTime.UtcNow - SentAt).TotalSeconds > ExpirySeconds;
        }

        // Keyed by target user id — a user only ever has one live invite popup, so a second
        // invite simply replaces whatever was on screen.
        private static readonly ConcurrentDictionary<int, PendingInvite> _pending = new();
        private static int _nextId = 0;

        public static bool Invite(GameClient senderSession, Group group, GameClient targetClient)
        {
            Habbo sender = senderSession?.GetHabbo();
            Habbo target = targetClient?.GetHabbo();
            if (sender == null || group == null || target == null)
                return false;

            if (_pending.TryGetValue(target.Id, out PendingInvite existing) && !existing.IsExpired) {
                senderSession.SendWhisper("That user already has a pending invite — let it expire first.", 1);
                return false;
            }

            PendingInvite invite = new()
            {
                Id = System.Threading.Interlocked.Increment(ref _nextId),
                GroupId = group.Id,
                SenderId = sender.Id,
                SenderName = sender.Username,
                TargetId = target.Id,
                SentAt = DateTime.UtcNow
            };

            _pending[target.Id] = invite;

            WebOverlay.SendGroupInvite(targetClient, invite.Id, sender.Username, group.Name, group.Id, group.Badge);
            return true;
        }

        public static void Respond(GameClient targetClient, int inviteId, bool accepted)
        {
            Habbo target = targetClient?.GetHabbo();
            if (target == null)
                return;

            if (!_pending.TryGetValue(target.Id, out PendingInvite invite) || invite.Id != inviteId)
                return;

            _pending.TryRemove(target.Id, out _);

            if (invite.IsExpired) {
                targetClient.SendWhisper("That group invite has expired.", 1);
                return;
            }

            GameClient senderSession = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(invite.SenderId);
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(invite.GroupId, out Group group) || group == null)
                return;

            if (!accepted) {
                senderSession?.SendWhisper(target.Username + " declined your invite to " + group.Name + ".", 1);
                targetClient.SendWhisper("You declined the invite to " + group.Name + ".", 1);
                ShoutFrom(target, DeclineShout(group));
                return;
            }

            // Re-check the guards — the world may have moved on during the 60s window.
            if (group.IsMember(target.Id)) {
                targetClient.SendWhisper("You're already a member of " + group.Name + ".", 1);
                return;
            }

            if (GroupManager.IsWorkableKind(group.Kind) && target.CorporationId != 0) {
                targetClient.SendWhisper("You already work for another corporation.", 1);
                senderSession?.SendWhisper(target.Username + " already works for another corporation.", 1);
                return;
            }

            Hire(senderSession, group, targetClient, target);
            ShoutFrom(target, AcceptShout(group));
        }

        public static void Hire(GameClient senderSession, Group group, GameClient targetClient, Habbo target)
        {
            if (group == null || target == null)
                return;

            if (target.GetStats().FavouriteGroupId == 0 && (group.Kind == GroupKind.Gang || group.Kind == GroupKind.Cartel || group.Kind == GroupKind.Mafia)) {
                target.GetStats().FavouriteGroupId = group.Id;
                int gid = group.Id;
                int uid = target.Id;
                using (WavePlusContext db = PlusEnvironment.GetDbContext())
                    db.UserStats.Where(x => x.Id == uid).ExecuteUpdate(s => s.SetProperty(x => x.Groupid, gid));

                if (target.InRoom && target.CurrentRoom != null) {
                    target.CurrentRoom.SendPacket(new RefreshFavouriteGroupComposer(target.Id));
                    target.CurrentRoom.SendPacket(new HabboGroupBadgesComposer(group));

                    RoomUser user = target.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(target.Id);
                    if (user != null)
                        target.CurrentRoom.SendPacket(new UpdateFavouriteGroupComposer(group, user.VirtualId));
                } else
                    targetClient?.SendPacket(new RefreshFavouriteGroupComposer(target.Id));
            }

            group.HandleRequest(target.Id, true);

            if (GroupManager.IsWorkableKind(group.Kind)) {
                target.CorporationId = group.Id;
                PlusEnvironment.GetGame().GetAchievementManager().QueueProgress(targetClient, "ACH_Hired", 1);
            }

            if (group.Kind == GroupKind.Gang)
                PlusEnvironment.GetGame().GetAchievementManager().QueueProgress(targetClient, "ACH_JoinGang", 1);

            if (senderSession?.GetHabbo() != null) {
                senderSession.SendPacket(new GroupMemberUpdatedComposer(group, target, 4));
                senderSession.SendPacket(new GroupInfoComposer(group, targetClient));
                LiveFeedService.LiveFeed(LiveFeedService.Name(senderSession.GetHabbo().Username, "green") + " hired " +
                    LiveFeedService.Name(target.Username, "lightblue") + " to <b>" + group.Name + "</b>");
            }

            targetClient?.SendWhisper("You joined " + group.Name + "!", 1);
            // The RP shout is left to the caller — the wording differs between accepting an
            // invite and being hired off an existing request.
        }

        // ── RP shout wording ────────────────────────────────────────────────
        // Corporations/Businesses (GroupKind 1 and 5) read as employment; every other kind
        // (gang, cartel, mafia, normal) reads as joining up.

        public static string InviteShout(Group group, string targetName) =>
            GroupManager.IsWorkableKind(group.Kind)
                ? "*sends " + targetName + " a hire offer to " + group.Name + "*"
                : "*invites " + targetName + " to join " + group.Name + "*";

        public static string AcceptShout(Group group) =>
            "*accepts the invitation to join " + group.Name + "*";

        public static string DeclineShout(Group group) =>
            "*declines the invitation to join " + group.Name + "*";

        private static void ShoutFrom(Habbo habbo, string message)
        {
            if (habbo == null || !habbo.InRoom || habbo.CurrentRoom == null)
                return;

            RoomUser user = habbo.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
            if (user != null)
                habbo.CurrentRoom.SendPacket(new ShoutComposer(user.VirtualId, message, 0, habbo.CustomBubbleId, isRpAction: true));
        }

        public static PendingInvite GetPending(int targetId)
        {
            if (_pending.TryGetValue(targetId, out PendingInvite invite) && !invite.IsExpired)
                return invite;

            return null;
        }

        public static void Clear(int userId) => _pending.TryRemove(userId, out _);
    }
}