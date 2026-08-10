using Plus.Communication.Packets.Outgoing.Roleplay.Gang;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Roleplay.Gang
{
    internal class RequestGangDataEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
                return;

            int groupId = packet.PopInt();
            session.SendPacket(new RPGangDataComposer(session, groupId));
        }
    }
}