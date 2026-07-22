namespace Plus.Communication.Packets.Outgoing.Notifications
{
    public class NuxAlertComposer : MessageComposer
    {
        private readonly string _jsonPayload;

        public NuxAlertComposer(string jsonPayload)
            : base(ServerPacketHeader.NuxAlertMessageComposer)
        {
            // Escape forward slashes the same way the Java plugin did
            _jsonPayload = jsonPayload.Replace("/", "&#47;");
        }

        public override void Compose(ServerPacket packet)
        {
            // Prefix expected by the overlay's openHabblet handler
            packet.WriteString("habblet/open/" + _jsonPayload);
        }
    }
}