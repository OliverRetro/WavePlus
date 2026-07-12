using Plus.HabboHotel.Achievements;

namespace Plus.Communication.Packets.Outgoing.Inventory.Achievements
{
    internal class AchievementProgressedComposer : MessageComposer
    {
        public Achievement Achievement { get; }
        public int TargetLevel { get; }
        public AchievementLevel TargetLevelData { get; }
        public int TotalLevels { get; }
        public UserAchievement UserData { get; }

        public AchievementProgressedComposer(Achievement achievement, int targetLevel, AchievementLevel targetLevelData, int totalLevels, UserAchievement userData)
            : base(ServerPacketHeader.AchievementProgressedMessageComposer)
        {
            Achievement = achievement;
            TargetLevel = targetLevel;
            TargetLevelData = targetLevelData;
            TotalLevels = totalLevels;
            UserData = userData;
        }

        public override void Compose(ServerPacket packet)
        {
            // Same cumulative translation as AchievementsComposer so single-level achievements show a
            // progress bar and completed ones read correctly (mirrors the Arcturus composer).
            int currentLevel = UserData != null ? UserData.Level : 0; // levels fully achieved
            bool hasNext = Achievement.Levels.ContainsKey(currentLevel + 1);
            bool hasAchieved = currentLevel >= TotalLevels;

            int displayLevel = hasNext ? currentLevel + 1 : (currentLevel > 0 ? currentLevel : 0);

            int currentThreshold = Achievement.GetCumulativeThreshold(currentLevel);
            int nextThreshold = hasNext ? Achievement.GetCumulativeThreshold(currentLevel + 1) : -1;
            int cumulativeProgress = currentThreshold + (UserData != null ? UserData.Progress : 0);

            packet.WriteInteger(Achievement.Id);
            packet.WriteInteger(displayLevel); // Target level
            packet.WriteString(Achievement.GroupName + displayLevel); // Target badge code
            packet.WriteInteger(currentThreshold); // Last level progress needed
            packet.WriteInteger(nextThreshold); // Progress needed
            packet.WriteInteger(hasNext ? Achievement.Levels[currentLevel + 1].RewardAmount : -1); // Reward amount
            packet.WriteInteger(hasNext ? Achievement.Levels[currentLevel + 1].PointsType : -1); // Reward currency type
            packet.WriteInteger(cumulativeProgress <= 0 ? 0 : cumulativeProgress); // Current progress
            packet.WriteBoolean(hasAchieved); // Achieved?
            packet.WriteString(Achievement.Category); // Category
            packet.WriteString(string.Empty);
            packet.WriteInteger(TotalLevels); // Total amount of levels
            packet.WriteInteger(hasAchieved ? 1 : 0); // 1 = progress bar visible when completed
        }
    }
}