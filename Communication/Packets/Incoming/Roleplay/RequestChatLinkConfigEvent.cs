using Plus.Communication.Packets.Outgoing.Roleplay;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Roleplay
{
    internal class RequestChatLinkConfigEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
                return;

            session.SendPacket(new ChatLinkConfigComposer(session));
            session.SendPacket(new ClientVisualSettingsComposer(session));
        }
    }
}