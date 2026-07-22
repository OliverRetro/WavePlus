namespace Plus.Communication.Packets.Outgoing.Camera
{
    internal class CameraStorageUrlComposer : MessageComposer
    {
        public string Url { get; }

        public CameraStorageUrlComposer(string url)
            : base(ServerPacketHeader.CameraPhotoPreviewComposer)
        {
            Url = url;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteString(Url);
        }
    }
}