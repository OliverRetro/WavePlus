namespace Plus.Communication.Packets.Outgoing.Camera
{
    internal class CameraPurchaseOkComposer : MessageComposer
    {
        public CameraPurchaseOkComposer()
            : base(ServerPacketHeader.CameraPhotoPurchaseOkComposer)
        {
        }

        public override void Compose(ServerPacket packet)
        {
        }
    }
}