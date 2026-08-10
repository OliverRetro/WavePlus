using System;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay
{
    internal class AddVipCommand : IChatCommand
    {
        public string PermissionRequired => "command_give_vip";

        public string Parameters => "%username% %duration (e.g. 30d, 12h, 1y) or 'remove'%";

        public string Description => "Grant, extend or remove timed VIP for a user.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 3) {
                session.SendWhisper("Usage: :vip <username> <duration|remove>  (units: s, m=minutes, h, d, w, mo=month, y)", 1);
                return;
            }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);
            if (targetClient?.GetHabbo() == null) {
                session.SendWhisper("That user could not be found (must be online).", 1);
                return;
            }

            var targetHabbo = targetClient.GetHabbo();

            if (string.Equals(@params[2], "remove", StringComparison.OrdinalIgnoreCase)
                || string.Equals(@params[2], "clear", StringComparison.OrdinalIgnoreCase)) {
                targetHabbo.ExpireVip();
                session.SendWhisper($"Removed VIP from {targetHabbo.Username}.", 1);
                targetClient.SendWhisper("Your VIP has been removed.", 1);
                return;
            }

            if (!TryParseDuration(@params[2], out int seconds)) {
                session.SendWhisper("Invalid duration. Examples: 30d, 12h, 90m, 1y.", 1);
                return;
            }

            targetHabbo.GrantVip(seconds);
            session.SendWhisper($"{targetHabbo.Username} now has {targetHabbo.VipMinutesRemaining} minute(s) of VIP.", 1);
            targetClient.SendWhisper("You have been given VIP!", 1);
        }

        // Parses "30d" / "12h" / "90m" / "1y" / "2w" / "45s" / "6mo" into seconds.
        private static bool TryParseDuration(string raw, out int seconds)
        {
            seconds = 0;
            if (string.IsNullOrWhiteSpace(raw))
                return false;

            raw = raw.Trim().ToLowerInvariant();

            int i = 0;
            while (i < raw.Length && char.IsDigit(raw[i]))
                i++;

            if (i == 0 || !int.TryParse(raw.Substring(0, i), out int amount) || amount <= 0)
                return false;

            string unit = raw.Substring(i);

            long factor = unit switch
            {
                "" or "d" => 86400,      // default unit is days
                "s" => 1,
                "m" => 60,               // minutes
                "h" => 3600,
                "w" => 604800,
                "mo" => 2592000,         // 30-day month
                "y" => 31536000,
                _ => 0
            };

            if (factor == 0)
                return false;

            long total = (long)amount * factor;
            if (total <= 0 || total > int.MaxValue)
                return false;

            seconds = (int)total;
            return true;
        }
    }
}