using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Groups
{
    internal class ManageGroupEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int groupId = packet.PopInt();

            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(groupId, out Group group))
                return;

            Habbo habbo = session.GetHabbo();
            if (habbo == null)
                return;

            if (!habbo.GetPermissions().HasRight("group_management_override") && !group.IsOwnerOrHasPermission(habbo.Id, "edit_group"))
                return;

            session.SendPacket(new ManageGroupComposer(group, group.Badge.Replace("b", "").Split('s')));
        }
    }
}