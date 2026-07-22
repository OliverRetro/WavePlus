namespace Plus.Communication.Packets.Outgoing.Rooms.Chat
{
    public class UserNftChatStylesComposer : MessageComposer
    {
        public int[] Bubbles { get; }

        public UserNftChatStylesComposer(int[] bubbles)
            : base(ServerPacketHeader.UserNftChatStylesMessageComposer)
        {
            Bubbles = bubbles;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Bubbles.Length);
            packet.WriteInteger(5);
            foreach (var bubbleId in Bubbles) {
                packet.WriteInteger(bubbleId);
            }
        }
    }
}