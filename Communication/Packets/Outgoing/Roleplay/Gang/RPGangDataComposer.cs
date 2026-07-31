using System;
using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.Cache.Type;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Outgoing.Roleplay.Gang
{
    internal class RPGangDataComposer : MessageComposer
    {
        private readonly GameClient _session;
        private readonly int _groupId;

        public RPGangDataComposer(GameClient session, int groupId = 0)
            : base(ServerPacketHeader.GangDataMessageComposer)
        {
            _session = session;
            _groupId = groupId;
        }

        public override void Compose(ServerPacket packet)
        {
            Habbo viewer = _session?.GetHabbo();
            GroupManager groups = PlusEnvironment.GetGame().GetGroupManager();

            Group gang = ResolveGang(viewer, groups);
            if (viewer == null || gang == null) {
                WriteSetupBranch(packet, viewer);
                return;
            }

            packet.WriteBoolean(true);

            packet.WriteInteger(gang.Id);
            packet.WriteString(gang.Name ?? string.Empty);
            packet.WriteString(gang.Description ?? string.Empty);
            packet.WriteInteger((int)gang.Kind);
            packet.WriteString(GangDefinition.ToHex(gang.Colour1));
            packet.WriteString(GangDefinition.ToHex(gang.Colour2));

            // TODO: gang stat tracking — level/xp/kills/etc. are not persisted yet.
            packet.WriteInteger(0);   // level
            packet.WriteInteger(0);   // experience
            packet.WriteInteger(100); // maxExperience
            packet.WriteInteger(0);   // kills
            packet.WriteInteger(0);   // copKills
            packet.WriteInteger(0);   // heists
            packet.WriteInteger(0);   // jailbreaks
            packet.WriteInteger(0);   // turfs
            packet.WriteInteger(0);   // earned

            bool isOwner = gang.CreatorId == viewer.Id;
            packet.WriteBoolean(gang.IsMember(viewer.Id));
            packet.WriteBoolean(isOwner);
            packet.WriteBoolean(isOwner || gang.HasPermission(viewer.Id, GangDefinition.PermEditLevels));
            packet.WriteBoolean(isOwner || gang.HasPermission(viewer.Id, GangDefinition.PermEditGroup));
            packet.WriteBoolean(isOwner || gang.HasPermission(viewer.Id, GangDefinition.PermGangInvite));
            packet.WriteBoolean(isOwner || gang.HasPermission(viewer.Id, GangDefinition.PermPromote));
            packet.WriteBoolean(isOwner || gang.HasPermission(viewer.Id, GangDefinition.PermDemote));
            packet.WriteBoolean(isOwner || gang.HasPermission(viewer.Id, GangDefinition.PermKick));

            List<GroupPermissionKey> keys = GangDefinition.ApplicableKeys(gang.Kind);
            packet.WriteInteger(keys.Count);
            foreach (GroupPermissionKey key in keys) {
                packet.WriteInteger(key.Id);
                packet.WriteString(key.Name ?? string.Empty);
            }

            List<GroupRole> roles = [.. gang.GetRoles.Where(r => r.Level >= 1).OrderBy(r => r.Level)];
            packet.WriteInteger(roles.Count);
            foreach (GroupRole role in roles) {
                packet.WriteInteger(role.Level);
                packet.WriteString(role.Name ?? string.Empty);

                List<int> enabled = [.. keys.Where(k => gang.RoleHasPermission(role.Level, k.Id)).Select(k => k.Id)];
                packet.WriteInteger(enabled.Count);
                foreach (int permId in enabled)
                    packet.WriteInteger(permId);
            }

            List<UserCache> members = PlusEnvironment.GetGame().GetCacheManager().GenerateUsers(gang.GetAllMembers);
            packet.WriteInteger(members.Count);
            foreach (UserCache member in members) {
                packet.WriteInteger(member.Id);
                packet.WriteString(member.Username ?? string.Empty);
                packet.WriteString(member.Look ?? string.Empty);
                packet.WriteInteger(gang.GetMemberLevel(member.Id));
            }
        }

        private Group ResolveGang(Habbo viewer, GroupManager groups)
        {
            if (viewer == null)
                return null;

            if (_groupId > 0)
                return groups.TryGetGroup(_groupId, out Group g) && g != null && GroupManager.IsGangKind(g.Kind) ? g : null;

            return groups.TryGetGangForUser(viewer.Id, out Group own) ? own : null;
        }

        private static void WriteSetupBranch(ServerPacket packet, Habbo viewer)
        {
            packet.WriteBoolean(false);
            packet.WriteInteger(GangDefinition.CreationCost());

            GroupInviteService.PendingInvite invite = viewer == null ? null : GroupInviteService.GetPending(viewer.Id);
            if (invite != null && PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(invite.GroupId, out Group group) && group != null) {
                packet.WriteInteger(1);
                packet.WriteInteger(invite.Id);
                packet.WriteString(group.Name ?? string.Empty);
                packet.WriteString(invite.SenderName ?? string.Empty);
                packet.WriteString(GangDefinition.ToHex(group.Colour1));
                packet.WriteString(GangDefinition.ToHex(group.Colour2));
                packet.WriteString(RelativeTime(invite.SentAt));
            } else {
                packet.WriteInteger(0);
            }
        }

        private static string RelativeTime(DateTime sentAtUtc)
        {
            TimeSpan delta = DateTime.UtcNow - sentAtUtc;
            if (delta.TotalSeconds < 60) return "just now";
            if (delta.TotalMinutes < 60) return (int)delta.TotalMinutes + "m ago";
            if (delta.TotalHours < 24) return (int)delta.TotalHours + "h ago";
            return (int)delta.TotalDays + "d ago";
        }
    }
}
