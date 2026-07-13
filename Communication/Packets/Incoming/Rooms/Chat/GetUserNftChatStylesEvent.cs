using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Chat
{
    public class GetUserNftChatStylesEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session?.GetHabbo() == null)
                return;

            int[] bubbles = session.GetHabbo().GetOwnedChatBubbleIds();

            session.GetHabbo().CurrentRoom.SendPacket(new UserNftChatStylesComposer(bubbles));
        }
    }
}