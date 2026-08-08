using Plus.Communication.Packets.Outgoing.Users;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Users
{
    internal class ScrGetUserInfoEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            session.SendPacket(new ScrSendUserInfoComposer(session.GetHabbo()));
        }
    }
}