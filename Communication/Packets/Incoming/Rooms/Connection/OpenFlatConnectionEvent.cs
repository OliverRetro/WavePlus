using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Connection
{
    public class OpenFlatConnectionEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null)
                return;

            int roomId = packet.PopInt();
            string password = packet.PopString();

            int hospitalRoomId = PlusEnvironment.GetHospitalManager().GetLoginRoomId(session.GetHabbo());
            if (hospitalRoomId > 0)
                roomId = hospitalRoomId;

            session.GetHabbo().PrepareRoom(roomId, password);
        }
    }
}