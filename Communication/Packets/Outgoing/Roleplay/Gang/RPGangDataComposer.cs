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
            packet.WriteInteger(gang.Colour1); // colour1: id into the group colorsA palette
            packet.WriteInteger(gang.Colour2); // colour2: id into the group colorsB palette

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

            List<GroupRole> roles = [.. gang.GetRoles
                .OrderBy(r => r.Level == 0 ? int.MinValue : -r.Level)];
            packet.WriteInteger(roles.Count);
            foreach (GroupRole role in roles) {
                packet.WriteInteger(role.Level);
                packet.WriteString(role.Name ?? string.Empty);

                List<int> enabled = role.Level == 0
                    ? []
                    : [.. keys.Where(k => gang.RoleHasPermission(role.Level, k.Id)).Select(k => k.Id)];
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

            // The gang badge (rendered on top of the two-tone colours, like normal groups).
            // Gangs created before badges existed have an empty code — fall back to the
            // default so they still show an emblem until the owner customises it.
            packet.WriteString(string.IsNullOrEmpty(gang.Badge) ? GangDefinition.DefaultBadge : gang.Badge);

            // Upgrade offer: only the owner can upgrade, and only if a higher tier exists.
            // We send the target tier's cost/kill gate + whether the viewer meets them so
            // the client shows the blue "Upgrade to X" button exactly when it's affordable.
            GroupKind? next = isOwner ? GangDefinition.NextKind(gang.Kind) : null;
            if (next.HasValue) {
                int cost = GangDefinition.CreationCost(next.Value);
                int killsRequired = GangDefinition.KillsRequired(next.Value);
                int kills = viewer.GetRpStats()?.Knockouts ?? 0;
                bool eligible = kills >= killsRequired && viewer.Credits >= cost;

                packet.WriteBoolean(true);
                packet.WriteInteger((int)next.Value);
                packet.WriteString(GangDefinition.DisplayName(next.Value));
                packet.WriteInteger(cost);
                packet.WriteInteger(killsRequired);
                packet.WriteBoolean(eligible);
            } else {
                packet.WriteBoolean(false);
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

            // The viewer's current knockouts (kills) + the per-kind creation gates, so
            // the create form can show requirements and disable ineligible types.
            packet.WriteInteger(viewer?.GetRpStats()?.Knockouts ?? 0);

            packet.WriteInteger(GangDefinition.CreatableKinds.Length);
            foreach (GroupKind kind in GangDefinition.CreatableKinds) {
                packet.WriteInteger((int)kind);
                packet.WriteInteger(GangDefinition.CreationCost(kind));
                packet.WriteInteger(GangDefinition.KillsRequired(kind));
            }

            GroupInviteService.PendingInvite invite = viewer == null ? null : GroupInviteService.GetPending(viewer.Id);
            if (invite != null && PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(invite.GroupId, out Group group) && group != null) {
                packet.WriteInteger(1);
                packet.WriteInteger(invite.Id);
                packet.WriteString(group.Name ?? string.Empty);
                packet.WriteString(invite.SenderName ?? string.Empty);
                packet.WriteInteger(group.Colour1);
                packet.WriteInteger(group.Colour2);
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