namespace Plus.Communication.Packets.Outgoing.Camera
{
    internal class InitCameraComposer : MessageComposer
    {
        public int CreditPrice { get; }
        public int PublishCreditPrice { get; }

        public InitCameraComposer(int creditPrice, int publishCreditPrice)
            : base(ServerPacketHeader.CameraPriceComposer)
        {
            CreditPrice = creditPrice;
            PublishCreditPrice = publishCreditPrice;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(CreditPrice);
            packet.WriteInteger(0);
            packet.WriteInteger(PublishCreditPrice);
        }
    }
}