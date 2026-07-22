using System.Diagnostics;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Handshake
{
    internal class PingEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            session.PingCount = 0;

            // Resolve an outstanding :ping. Anything else is the 30s keepalive, which
            // leaves PingRequestedAt at 0 and so falls straight through.
            long requestedAt = session.PingRequestedAt;
            if (requestedAt == 0)
                return;

            session.PingRequestedAt = 0;

            double elapsedMs = (Stopwatch.GetTimestamp() - requestedAt) * 1000d / Stopwatch.Frequency;

            session.SendWhisper("Pong! " + elapsedMs.ToString("0.0") + "ms round trip.", 1);
        }
    }
}