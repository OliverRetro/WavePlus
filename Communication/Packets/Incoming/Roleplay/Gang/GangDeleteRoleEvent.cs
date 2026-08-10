using Plus.Communication.Packets.Outgoing.Roleplay.Gang;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Roleplay.Gang
{
    internal class GangDeleteRoleEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            Habbo viewer = session?.GetHabbo();
            if (viewer == null)
                return;

            int roleLevel = packet.PopInt();

            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGangForUser(viewer.Id, out Group gang) || gang == null)
                return;

            bool canManage = gang.CreatorId == viewer.Id || gang.HasPermission(viewer.Id, GangDefinition.PermEditLevels);
            if (!canManage) {
                session.SendPacket(new RPGangNoticeComposer("You don't have permission to manage roles."));
                return;
            }

            if (!gang.RemoveGangRole(roleLevel)) {
                session.SendPacket(new RPGangNoticeComposer("That role can't be deleted."));
                return;
            }

            gang.SaveRoles();

            session.SendPacket(new RPGangNoticeComposer("Role deleted."));
            session.SendPacket(new RPGangDataComposer(session));
        }
    }
}