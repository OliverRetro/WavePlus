namespace Plus.HabboHotel.Moderation
{
    public class ModerationBan
    {
        public string Value { get; set; }
        public double Expire { get; set; }
        public string Reason { get; set; }
        public ModerationBanType Type { get; set; }
        public int UserId { get; set; }

        public ModerationBan(ModerationBanType type, string value, string reason, double expire, int userId = 0)
        {
            Type = type;
            Value = value;
            Reason = reason;
            Expire = expire;
            UserId = userId;
        }

        public bool Expired => PlusEnvironment.GetUnixTimestamp() >= Expire;
    }
}