using System.Collections.Generic;

namespace Plus.HabboHotel.Achievements
{
    public class Achievement
    {
        public int Id { get; }
        public string Category { get; }
        public string GroupName { get; }
        public int GameId { get; }

        public Dictionary<int, AchievementLevel> Levels;

        public Achievement(int id, string groupName, string category, int gameId)
        {
            Id = id;
            GroupName = groupName;
            Category = category;
            GameId = gameId;
            Levels = new Dictionary<int, AchievementLevel>();
        }

        public void AddLevel(AchievementLevel level)
        {
            Levels.Add(level.Level, level);
        }

        /// <summary>
        /// The cumulative progress required to have fully reached <paramref name="level"/> — i.e. the
        /// sum of every level's per-level requirement up to and including it. Our model stores progress
        /// per-level (reset on level-up); the client expects cumulative totals, so composers translate
        /// through this. Level 0 (nothing achieved yet) is a threshold of 0.
        /// </summary>
        public int GetCumulativeThreshold(int level)
        {
            int total = 0;
            for (int l = 1; l <= level && Levels.ContainsKey(l); l++)
                total += Levels[l].Requirement;

            return total;
        }
    }
}