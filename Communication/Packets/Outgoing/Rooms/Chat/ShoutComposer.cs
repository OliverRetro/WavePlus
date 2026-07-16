namespace Plus.Communication.Packets.Outgoing.Rooms.Chat
{
    public class ShoutComposer : MessageComposer
    {
        public int VirtualId { get; }
        public string Message { get; }
        public int Emotion { get; }
        public int Colour { get; }
        public bool IsRpAction { get; }

        public ShoutComposer(int virtualId, string message, int emotion, int colour, bool isRpAction = false)
            : base(ServerPacketHeader.ShoutMessageComposer)
        {
            VirtualId = virtualId;
            Message = message;
            Emotion = emotion;
            Colour = colour;
            IsRpAction = isRpAction;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(VirtualId);
            packet.WriteString(Message);
            packet.WriteInteger(Emotion);
            packet.WriteInteger(Colour);
            packet.WriteInteger(0);
            packet.WriteInteger(-1);
            // WaveRP: trailing flag marks RP-action bubbles so the client renders
            // "*Username: action*" without the server baking the username into the text.
            packet.WriteInteger(IsRpAction ? 1 : 0);
        }
    }
}