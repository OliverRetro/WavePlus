namespace Plus.Communication.Packets.Outgoing.Roleplay
{
    public class GameClockComposer : MessageComposer
    {
        private readonly int _offsetSeconds;

        public GameClockComposer(int offsetSeconds)
            : base(ServerPacketHeader.GameClockMessageComposer)
        {
            _offsetSeconds = offsetSeconds;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(_offsetSeconds);
        }
    }
}