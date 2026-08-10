namespace Plus.HabboHotel.Moderation
{
    public static class BanTypeUtility
    {
        public static ModerationBanType GetModerationBanType(string type)
        {
            switch (type) {
                case "ip":
                    return ModerationBanType.Ip;
                default:
                case "account":
                    return ModerationBanType.Username;
            }
        }

        public static string FromModerationBanType(ModerationBanType type)
        {
            switch (type) {
                case ModerationBanType.Ip:
                    return "ip";
                default:
                    return "account";
            }
        }
    }
}