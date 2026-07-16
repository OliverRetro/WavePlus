using System.Collections.Generic;
using System.Linq;
using Plus.Database.EF;

namespace Plus.HabboHotel.Achievements
{
    public static class AchievementLevelFactory
    {
        public static void GetAchievementLevels(out Dictionary<string, Achievement> achievements)
        {
            achievements = [];

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            var rows = db.Achievements.Where(a => a.Enabled == 1).Select(a => new { a.Id, a.Category, a.GroupName, a.Level, a.RewardAmount, a.PointsType, a.RewardPoints, a.ProgressNeeded, a.GameId }).ToList();
            foreach (var row in rows)
            {
                int id = (int)row.Id;
                string category = row.Category;
                string groupName = row.GroupName;

                AchievementLevel level = new(row.Level, row.RewardAmount, row.PointsType, row.RewardPoints, row.ProgressNeeded);

                if (!achievements.TryGetValue(groupName, out Achievement value))
                {
                    Achievement achievement = new(id, groupName, category, row.GameId);
                    achievement.AddLevel(level);
                    achievements.Add(groupName, achievement);
                }
                else
                    value.AddLevel(level);
            }
        }
    }
}