namespace Plus.Communication.Packets.Outgoing.Camera
{
    internal class ThumbnailStatusComposer(bool ok, bool renderLimitHit = false) : MessageComposer(ServerPacketHeader.SendRoomThumbnailAlertMessageComposer)
    {
        public bool Ok { get; } = ok;
        public bool RenderLimitHit { get; } = renderLimitHit;

        public override void Compose(ServerPacket packet)
        {
            packet.WriteBoolean(Ok);
            packet.WriteBoolean(RenderLimitHit);
        }
    }
}