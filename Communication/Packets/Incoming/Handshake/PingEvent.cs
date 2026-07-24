using System.Diagnostics;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Handshake
{
    internal class PingEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            session.PingCount = 0;

            long now = Stopwatch.GetTimestamp();

            // The 30s keep-alive doubles as a free latency probe — combat needs a live round-trip
            // figure to rewind the world to the frame the attacker swung at.
            long keepAliveAt = session.KeepAlivePingAt;
            if (keepAliveAt != 0) {
                session.KeepAlivePingAt = 0;
                session.RecordLatencySample(ElapsedMs(keepAliveAt, now));
            }

            // Resolve an outstanding :ping. Anything else is the keep-alive, which leaves
            // PingRequestedAt at 0 and so falls straight through.
            long requestedAt = session.PingRequestedAt;
            if (requestedAt == 0)
                return;

            session.PingRequestedAt = 0;

            double elapsedMs = ElapsedMs(requestedAt, now);
            session.RecordLatencySample(elapsedMs);

            session.SendWhisper("Pong! " + elapsedMs.ToString("0.0") + "ms round trip.", 1);
        }

        private static double ElapsedMs(long fromTimestamp, long toTimestamp) =>
            (toTimestamp - fromTimestamp) * 1000d / Stopwatch.Frequency;
    }
}