using System.Net;
using DotNetty.Codecs.Http;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Plus.HabboHotel.GameClients;

namespace Plus.Network.Handler
{
    internal class RealIpHandler : ChannelHandlerAdapter
    {
        private static readonly AsciiString RealIpHeader = new("X-Real-IP");

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IFullHttpRequest request) {
                string ip = request.Headers.TryGet(RealIpHeader, out ICharSequence header) ? header?.ToString() : null;

                if (string.IsNullOrWhiteSpace(ip) && context.Channel.RemoteAddress is IPEndPoint remote)
                    ip = remote.Address.MapToIPv4().ToString();

                if (!string.IsNullOrWhiteSpace(ip) &&
                    PlusEnvironment.GetGame().GetClientManager().TryGetClient(context.Channel.Id, out GameClient client) &&
                    client != null)
                    client.IpAddress = ip.Trim();

                context.Channel.Pipeline.Remove(this);
            }

            context.FireChannelRead(message);
        }
    }
}