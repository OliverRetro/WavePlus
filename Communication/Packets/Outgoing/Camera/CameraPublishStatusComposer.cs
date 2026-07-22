namespace Plus.Communication.Packets.Outgoing.Camera
{
    internal class CameraPublishStatusComposer : MessageComposer
    {
        public bool Ok { get; }
        public int SecondsToWait { get; }
        public string Url { get; }

        public CameraPublishStatusComposer(bool ok, int secondsToWait, string url = "")
            : base(ServerPacketHeader.CameraPublishStatusMessageComposer)
        {
            Ok = ok;
            SecondsToWait = secondsToWait;
            Url = url;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteBoolean(Ok);
            packet.WriteInteger(SecondsToWait);
            packet.WriteString(Url);
        }
    }
}