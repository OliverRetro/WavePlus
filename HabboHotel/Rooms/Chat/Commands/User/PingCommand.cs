using System.Diagnostics;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class PingCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_ping";

        public string Parameters => "";

        public string Description => "Check your connection round trip time to the server.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null)
                return;

            if (session.PingRequestedAt != 0) {
                session.SendWhisper("A ping is already in flight - hang on a moment.", 1);
                return;
            }

            session.PingRequestedAt = Stopwatch.GetTimestamp();
            session.SendPacket(new PongComposer());
        }
    }
}