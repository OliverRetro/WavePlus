using System;

namespace Plus.HabboHotel.Roleplay.GameClock
{
    public static class GameClockService
    {
        public const int Scale = 15;

        private const int SecondsPerDay = 86400;
        private const string SettingKey = "game.clock.offset";

        public static int GetOffset()
        {
            string raw = PlusEnvironment.GetSettingsManager().TryGetValue(SettingKey);
            return int.TryParse(raw, out int offset) ? offset : 0;
        }

        public static int SetClock(int hour, int minute)
        {
            long nowSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long baseSeconds = (nowSeconds * Scale) % SecondsPerDay;
            int desired = ((hour * 3600) + (minute * 60)) % SecondsPerDay;

            int offset = (int)(desired - baseSeconds);

            PlusEnvironment.GetSettingsManager().SetValue(SettingKey, offset.ToString(),
                "Global ingame day/night clock offset (seconds), set via :settime.");

            return offset;
        }
    }
}