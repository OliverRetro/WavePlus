namespace Plus.Communication.Packets.Outgoing.Rooms.Chat
{
    public class MentionWhisperComposer : MessageComposer
    {
        public const int MentionFlag = 2;

        public int VirtualId { get; }
        public string Text { get; }
        public int Colour { get; }

        public MentionWhisperComposer(int virtualId, string text, int colour)
            : base(ServerPacketHeader.WhisperMessageComposer)
        {
            VirtualId = virtualId;
            Text = text;
            Colour = colour;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(VirtualId);
            packet.WriteString(Text);
            packet.WriteInteger(0);
            packet.WriteInteger(Colour);

            packet.WriteInteger(0);
            packet.WriteInteger(-1);

            packet.WriteInteger(MentionFlag);
        }
    }
}
