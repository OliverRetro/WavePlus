using System.Linq;
using Plus.Database.EF;
using Plus.Database.EF.Entities;

namespace Plus.HabboHotel.Roleplay.Settings
{
    public sealed class UserRpSettings
    {
        public int UserId { get; }
        public bool ClickThrough { get; set; }
        public bool AutoSalaryDeposit { get; set; }
        public bool FurnitureTrading { get; set; }
        public bool Livefeed { get; set; }
        public bool LinkWarning { get; set; } = true;
        public bool AutoEventTransfer { get; set; }
        public bool VoiceChat { get; set; }
        public bool Alerts { get; set; }
        public bool ChatHigher { get; set; }
        public int FpsMax { get; set; } = FpsMaxDefault;
        public bool DragRooms { get; set; } = true;

        public const int FpsMaxDefault = 24;
        public const int FpsMaxCap = 240;

        public static int ClampFps(int value) => value < FpsMaxDefault ? FpsMaxDefault : (value > FpsMaxCap ? FpsMaxCap : value);

        public UserRpSettings(int userId, bool clickThrough, bool autoSalaryDeposit, bool furnitureTrading,
            bool autoEventTransfer, bool voiceChat, bool livefeed, bool alerts, bool linkWarning,
            bool chatHigher, int fpsMax, bool dragRooms)
        {
            UserId = userId;
            ClickThrough = clickThrough;
            AutoSalaryDeposit = autoSalaryDeposit;
            FurnitureTrading = furnitureTrading;
            AutoEventTransfer = autoEventTransfer;
            VoiceChat = voiceChat;
            Livefeed = livefeed;
            Alerts = alerts;
            LinkWarning = linkWarning;
            ChatHigher = chatHigher;
            FpsMax = ClampFps(fpsMax);
            DragRooms = dragRooms;
        }

        public static UserRpSettings Load(int userId)
        {
            using WavePlusContext db = PlusEnvironment.GetDbContext();

            UserRpSettingEntity row = db.UserRpSettings.FirstOrDefault(s => s.UserId == userId);
            if (row == null) {
                row = new UserRpSettingEntity { UserId = userId };
                db.UserRpSettings.Add(row);
                db.SaveChanges();
            }

            return new UserRpSettings(
                userId,
                row.ClickthroughEnabled == 1,
                row.AutoSalaryDeposit == 1,
                row.FurnitureTradingEnabled == 1,
                row.AutoEventTransfer == 1,
                row.VoiceChatEnabled == 1,
                row.LivefeedEnabled == 1,
                row.AlertsEnabled == 1,
                row.LinkWarningEnabled == 1,
                row.ChatHigherEnabled == 1,
                row.FpsMax,
                row.DragRoomsEnabled == 1);
        }

        public void Save()
        {
            using WavePlusContext db = PlusEnvironment.GetDbContext();

            UserRpSettingEntity row = db.UserRpSettings.FirstOrDefault(s => s.UserId == UserId);
            if (row == null) {
                row = new UserRpSettingEntity { UserId = UserId };
                db.UserRpSettings.Add(row);
            }

            row.ClickthroughEnabled = (sbyte)(ClickThrough ? 1 : 0);
            row.AutoSalaryDeposit = (sbyte)(AutoSalaryDeposit ? 1 : 0);
            row.FurnitureTradingEnabled = (sbyte)(FurnitureTrading ? 1 : 0);
            row.AutoEventTransfer = (sbyte)(AutoEventTransfer ? 1 : 0);
            row.VoiceChatEnabled = (sbyte)(VoiceChat ? 1 : 0);
            row.LivefeedEnabled = (sbyte)(Livefeed ? 1 : 0);
            row.AlertsEnabled = (sbyte)(Alerts ? 1 : 0);
            row.LinkWarningEnabled = (sbyte)(LinkWarning ? 1 : 0);
            row.ChatHigherEnabled = (sbyte)(ChatHigher ? 1 : 0);
            row.FpsMax = ClampFps(FpsMax);
            row.DragRoomsEnabled = (sbyte)(DragRooms ? 1 : 0);

            db.SaveChanges();
        }
    }
}