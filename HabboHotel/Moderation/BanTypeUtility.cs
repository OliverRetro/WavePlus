namespace Plus.HabboHotel.Moderation
{
    public static class BanTypeUtility
    {
        public static ModerationBanType GetModerationBanType(string type)
        {
            switch (type) {
                default:
                case "user":
                    return ModerationBanType.Username;
                case "ip":
                    return ModerationBanType.Ip;
                case "machine":
                    return ModerationBanType.Machine;
            }
        }

        public static string FromModerationBanType(ModerationBanType type)
        {
            switch (type) {
                default:
                case ModerationBanType.Username:
                    return "user";
                case ModerationBanType.Ip:
                    return "ip";
                case ModerationBanType.Machine:
                    return "machine";
            }
        }
    }
}