using System.Collections.Generic;

namespace Plus.HabboHotel.Achievements
{
    public class Achievement(int id, string groupName, string category, int gameId)
    {
        public int Id { get; } = id;
        public string Category { get; } = category;
        public string GroupName { get; } = groupName;
        public int GameId { get; } = gameId;

        public Dictionary<int, AchievementLevel> Levels = [];

        public void AddLevel(AchievementLevel level)
        {
            Levels.Add(level.Level, level);
        }

        public int GetCumulativeThreshold(int level)
        {
            int total = 0;
            for (int l = 1; l <= level && Levels.ContainsKey(l); l++)
                total += Levels[l].Requirement;

            return total;
        }
    }
}