using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Incoming.Groups
{
    internal class DeclineGroupMembershipEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int groupId = packet.PopInt();
            int userId = packet.PopInt();

            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(groupId, out Group group))
                return;

            if (!group.IsOwnerOrHasPermission(session.GetHabbo().Id, "hire_user") && !session.GetHabbo().GetPermissions().HasRight("fuse_group_accept_any"))
                return;

            if (!group.HasRequest(userId))
                return;

            group.HandleRequest(userId, false);
            session.SendPacket(new UnknownGroupComposer(group.Id, userId));
        }
    }
}