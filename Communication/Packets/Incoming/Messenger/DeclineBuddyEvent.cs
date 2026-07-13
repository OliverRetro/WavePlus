using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Messenger
{
    internal class DeclineBuddyEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || session.GetHabbo().GetMessenger() == null)
                return;

            bool declineAll = packet.PopBoolean();
            packet.PopInt(); //amount

            if (!declineAll)
            {
                int requestId = packet.PopInt();
                session.GetHabbo().GetMessenger().HandleRequest(requestId);
            }
            else
                session.GetHabbo().GetMessenger().HandleAllRequests();
        }
    }
}