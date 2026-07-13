using Plus.Communication.Packets.Outgoing.Rooms.Settings;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    internal class UnbanUserFromRoomEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
                return;

            Room instance = session.GetHabbo().CurrentRoom;
            if (instance == null || !instance.CheckRights(session, true))
                return;

            int userId = packet.PopInt();
            int roomId = packet.PopInt();

            if (instance.GetBans().IsBanned(userId))
            {
                instance.GetBans().Unban(userId);

                session.SendPacket(new UnbanUserFromRoomComposer(roomId, userId));
            }
        }
    }
}