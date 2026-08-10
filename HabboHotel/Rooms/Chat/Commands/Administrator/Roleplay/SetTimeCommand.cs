using Plus.Communication.Packets.Outgoing.Roleplay;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.GameClock;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay
{
    internal class SetTimeCommand : IChatCommand
    {
        public string PermissionRequired => "command_set_time";

        public string Parameters => "%hh:mm%";

        public string Description => "Set the global ingame clock for everyone (e.g. :settime 16:30).";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 2) {
                session.SendWhisper("Usage: :settime HH:MM (24h), e.g. :settime 06:30");
                return;
            }

            string[] parts = @params[1].Split(':');
            if (parts.Length != 2 ||
                !int.TryParse(parts[0], out int hour) ||
                !int.TryParse(parts[1], out int minute) ||
                hour < 0 || hour > 23 || minute < 0 || minute > 59) {
                session.SendWhisper("Invalid time. Use HH:MM in 24h format, e.g. :settime 18:45");
                return;
            }

            int offset = GameClockService.SetClock(hour, minute);

            // Push the new offset to every online client so their clock and
            // background shift immediately.
            foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients) {
                if (client?.GetHabbo() == null)
                    continue;

                client.SendPacket(new GameClockComposer(offset));
            }

            session.SendWhisper($"Ingame time set to {hour:D2}:{minute:D2} for everyone.");
        }
    }
}