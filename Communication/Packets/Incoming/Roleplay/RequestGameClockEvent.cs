using Plus.Communication.Packets.Outgoing.Roleplay;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.GameClock;

namespace Plus.Communication.Packets.Incoming.Roleplay
{
    internal class RequestGameClockEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
                return;

            session.SendPacket(new GameClockComposer(GameClockService.GetOffset()));
        }
    }
}