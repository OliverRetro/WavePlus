using Plus.Communication.Packets.Outgoing.Roleplay;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Roleplay
{
    internal class RequestWantedListEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
                return;

            session.SendPacket(new WantedListComposer());
        }
    }
}