using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.HabboHotel.GameClients;
using System;

namespace Plus.Communication.Packets.Incoming.Rooms.Connection
{
    internal class GoToFlatEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom) {
                Console.WriteLine("User isn't in room obviously so couldnt GoToFlat");
                return;
            }

            if (!session.GetHabbo().EnterRoom(session.GetHabbo().CurrentRoom)) {
                Console.WriteLine("User !EnterRoom GoToFlat");
                session.SendPacket(new CloseConnectionComposer());
            }
        }
    }
}