using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Incoming.Groups
{
    internal class SetGroupFavouriteEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null)
                return;

            int groupId = packet.PopInt();
            if (groupId == 0)
                return;

            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(groupId, out Group group))
                return;

            // Intentionally a no-op: favourite-group handling is done elsewhere.
        }
    }
}