using System;
using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Plus.Communication.Packets.Incoming;

namespace Plus.Network.Codec
{
    public class GameDecoder : MessageToMessageDecoder<IByteBuffer>
    {
        protected override void Decode(IChannelHandlerContext context, IByteBuffer message, List<object> output)
        {
            try {
                short id = message.ReadShort();
                ClientPacket packet = new(id, message.ReadBytes(message.ReadableBytes));

                output.Add(packet);
            } catch (Exception) {
            }
        }
    }
}