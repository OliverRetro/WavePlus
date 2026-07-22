namespace Plus.HabboHotel.Roleplay.Banking
{
    public class UserRpBankLog
    {
        public int UserId { get; }
        public int Amount { get; }
        public string ActionType { get; }
        public string ManagementType { get; }
        public int FeePaid { get; }
        public int Timestamp { get; }

        public UserRpBankLog(int userId, int amount, string actionType, string managementType, int feePaid, int timestamp)
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