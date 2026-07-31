using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Roleplay.Gang;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Roleplay.Gang
{
    internal class GangSaveRoleEvent : IPacketEvent
    {
        private const int MaxNameLength = 24;

        public void Parse(GameClient session, ClientPacket packet)
        {
            Habbo viewer = session?.GetHabbo();
            if (viewer == null)
                return;

            int roleLevel = packet.PopInt();
            string name = (packet.PopString() ?? string.Empty).Trim();

            int permCount = packet.PopInt();
            HashSet<int> enabled = new();
            for (int i = 0; i < permCount; i++)
                enabled.Add(packet.PopInt());

            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGangForUser(viewer.Id, out Group gang) || gang == null)
                return;

            bool canManage = gang.CreatorId == viewer.Id || gang.HasPermission(viewer.Id, GangDefinition.PermEditLevels);
            if (!canManage) {
                session.SendPacket(new RPGangNoticeComposer("You don't have permission to manage roles."));
                return;
            }

            if (string.IsNullOrWhiteSpace(name)) {
                session.SendPacket(new RPGangNoticeComposer("Enter a role name."));
                return;
            }

            if (name.Length > MaxNameLength)
                name = name.Substring(0, MaxNameLength);

            int level;
            if (roleLevel == 0) {
                GroupRole role = gang.AddRole(name, 0, string.Empty, string.Empty, 5, true);
                level = role.Level;
            } else {
                if (roleLevel < 1 || !gang.UpdateRole(roleLevel, name, 0, string.Empty, 5, string.Empty)) {
                    session.SendPacket(new RPGangNoticeComposer("That role no longer exists."));
                    return;
                }
                level = roleLevel;
            }

            // Apply only the keys valid for this kind; an unchecked key removes its row.
            foreach (GroupPermissionKey key in GangDefinition.ApplicableKeys(gang.Kind))
                gang.SetPermission(level, key.Id, enabled.Contains(key.Id));

            gang.SaveRoles();
            gang.SavePermissions();

            session.SendPacket(new RPGangNoticeComposer($"Role \"{name}\" saved."));
            session.SendPacket(new RPGangDataComposer(session));
        }
    }
}
