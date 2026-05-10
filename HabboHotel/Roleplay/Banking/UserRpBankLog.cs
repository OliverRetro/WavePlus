using System;

namespace Plus.HabboHotel.Roleplay.Banking
{
    public class UserRpBankLog
    {
        public int UserId { get; }
        public int Amount { get; }
        public string ActionType { get; }
        public string ManagementType { get; }
        public int FeePaid { get; }
        public DateTime Timestamp { get; }

        public UserRpBankLog(int userId, int amount, string actionType, string managementType, int feePaid, DateTime timestamp)
        {
            UserId = userId;
            Amount = amount;
            ActionType = actionType;
            ManagementType = managementType;
            FeePaid = feePaid;
            Timestamp = timestamp;
        }
    }
}
