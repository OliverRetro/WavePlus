using System.Linq;
using Plus.Database.EF;
using Plus.Database.EF.Entities;

namespace Plus.HabboHotel.Roleplay.Settings
{
    public sealed class UserRpSettings
    {
        public int UserId { get; }

        // Implemented behaviours.
        public bool ClickThrough { get; set; }
        public bool AutoSalaryDeposit { get; set; }
        public bool FurnitureTrading { get; set; }
        public bool Livefeed { get; set; }
        public bool LinkWarning { get; set; } = true;

        // Persisted + overlay round-tripped, not wired into gameplay yet.
        public bool AutoEventTransfer { get; set; }
        public bool VoiceChat { get; set; }
        public bool Alerts { get; set; }

        public UserRpSettings(int userId, bool clickThrough, bool autoSalaryDeposit, bool furnitureTrading,
            bool autoEventTransfer, bool voiceChat, bool livefeed, bool alerts, bool linkWarning)
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
                row.LinkWarningEnabled == 1);
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

            db.SaveChanges();
        }
    }
}