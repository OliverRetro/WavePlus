using System.Text.Json;

namespace Plus.Communication.Packets.Outgoing.Overlay
{
    public class WebOverlayComposer : MessageComposer
    {
        private readonly string _json;

        public WebOverlayComposer(string header, object data)
            : base(ServerPacketHeader.NuxAlertMessageComposer)
        {
            var envelope = new { header, data };
            _json = JsonSerializer.Serialize(envelope).Replace("/", "&#47;");
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteString("habblet/open/" + _json);
        }
    }
}