using System;
using System.IO;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using log4net;
using Plus.Communication.Packets.Incoming;

namespace Plus.Network
{
    public class NetworkChannelHandler : SimpleChannelInboundHandler<ClientPacket>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NetworkChannelHandler));

        public override void ChannelActive(IChannelHandlerContext context)
        {
            PlusEnvironment.GetGame().GetClientManager().CreateAndStartClient(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            PlusEnvironment.GetGame().GetClientManager().DisposeConnection(context.Channel.Id);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            if (exception is IOException) return;
            Log.Debug(exception.Message);
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, ClientPacket msg)
        {
            if (PlusEnvironment.GetGame().GetClientManager().TryGetClient(ctx.Channel.Id, out var client))
                PlusEnvironment.GetGame().GetPacketManager().TryExecutePacket(client, msg);
        }

        public override void ChannelReadComplete(IChannelHandlerContext ctx)
        {
            ctx.Flush();
        }

        public override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            if (evt.GetType().IsInstanceOfType(ChannelInputShutdownEvent.Instance))
            {
                context.CloseAsync();
            }
        }

        public override bool IsSharable => true;
    }
}