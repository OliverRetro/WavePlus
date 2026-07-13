using System.Collections.Generic;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Outgoing.Inventory.Achievements
{
    internal class AchievementsComposer(Habbo habbo, List<Achievement> achievements) : MessageComposer(ServerPacketHeader.AchievementsMessageComposer)
    {
        public List<Achievement> Achievements { get; } = achievements;
        public Habbo Habbo { get; } = habbo;

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Achievements.Count);
            foreach (Achievement achievement in Achievements)
            {
                UserAchievement userData = Habbo.GetAchievementData(achievement.GroupName);
                int totalLevels = achievement.Levels.Count;
                int currentLevel = userData != null ? userData.Level : 0; // levels fully achieved
                bool hasNext = achievement.Levels.ContainsKey(currentLevel + 1);
                bool hasAchieved = currentLevel >= totalLevels;
                int displayLevel = hasNext ? currentLevel + 1 : (currentLevel > 0 ? currentLevel : 0);

                int currentThreshold = achievement.GetCumulativeThreshold(currentLevel);
                int nextThreshold = hasNext ? achievement.GetCumulativeThreshold(currentLevel + 1) : -1;
                int cumulativeProgress = currentThreshold + (userData != null ? userData.Progress : 0);

                packet.WriteInteger(achievement.Id);
                packet.WriteInteger(displayLevel);
                packet.WriteString(achievement.GroupName + displayLevel);
                packet.WriteInteger(currentThreshold);
                packet.WriteInteger(nextThreshold);
                packet.WriteInteger(hasNext ? achievement.Levels[currentLevel + 1].RewardAmount : -1);
                packet.WriteInteger(hasNext ? achievement.Levels[currentLevel + 1].PointsType : -1);
                packet.WriteInteger(cumulativeProgress <= 0 ? 0 : cumulativeProgress);
                packet.WriteBoolean(hasAchieved); // Achieved?
                packet.WriteString(achievement.Category); // Category
                packet.WriteString(string.Empty);
                packet.WriteInteger(totalLevels);
                packet.WriteInteger(hasAchieved ? 1 : 0);
            }

            packet.WriteString("");
        }
    }
}