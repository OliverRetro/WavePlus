using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    internal class LetUserInEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out Room room))
                return;

            if (!room.CheckRights(session))
                return;

            string name = packet.PopString();
            bool accepted = packet.PopBoolean();

            GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(name);
            if (client == null)
                return;

            if (accepted)
            {
                client.GetHabbo().RoomAuthOk = true;
                client.SendPacket(new FlatAccessibleComposer(""));
                room.SendPacket(new FlatAccessibleComposer(client.GetHabbo().Username), true);
            }
            else
            {
                client.SendPacket(new FlatAccessDeniedComposer(""));
                room.SendPacket(new FlatAccessDeniedComposer(client.GetHabbo().Username), true);
            }
        }
    }
}