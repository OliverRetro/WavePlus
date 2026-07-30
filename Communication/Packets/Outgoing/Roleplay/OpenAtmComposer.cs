namespace Plus.Communication.Packets.Outgoing.Roleplay
{
    public class OpenAtmComposer : MessageComposer
    {
        private readonly int _withdrawFeePercent;
        private readonly int _depositFeePercent;

        public OpenAtmComposer(int withdrawFeePercent, int depositFeePercent)
            : base(ServerPacketHeader.OpenAtmMessageComposer)
        {
            _withdrawFeePercent = withdrawFeePercent;
            _depositFeePercent = depositFeePercent;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(_withdrawFeePercent);
            packet.WriteInteger(_depositFeePercent);
        }
    }
}