using System.Collections.Generic;
using System.Linq;
using Plus.Database.EF;

namespace Plus.HabboHotel.Achievements
{
    public static class AchievementLevelFactory
    {
        public static void GetAchievementLevels(out Dictionary<string, Achievement> achievements)
        {
            achievements = new Dictionary<string, Achievement>();

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var rows = db.achievements.Where(a => a.enabled == 1).Select(a => new { a.id, a.category, a.group_name, a.level, a.reward_pixels, a.reward_points, a.progress_needed, a.game_id }).ToList();
                foreach (var row in rows)
                {
                    int id = (int)row.id;
                    string category = row.category;
                    string groupName = row.group_name;

                    AchievementLevel level = new(row.level, row.reward_pixels, row.reward_points, row.progress_needed);

                    if (!achievements.ContainsKey(groupName)) {
                        Achievement achievement = new(id, groupName, category, row.game_id);
                        achievement.AddLevel(level);
                        achievements.Add(groupName, achievement);
                    } else
                        achievements[groupName].AddLevel(level);
                }
            }
        }
    }
}