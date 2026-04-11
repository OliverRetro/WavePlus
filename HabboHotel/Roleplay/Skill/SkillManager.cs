using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using log4net;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Roleplay.Skill;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Permissions
{
    public sealed class SkillManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SkillManager));

        private readonly Dictionary<int, Skill> _skills = new();

        public void Init()
        {
            _skills.Clear();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                // Load skills
                dbClient.SetQuery("SELECT * FROM `rp_skills`");
                DataTable getSkills = dbClient.GetTable();

                if (getSkills != null)
                {
                    foreach (DataRow row in getSkills.Rows)
                    {
                        var skill = new Skill(
                            Convert.ToInt32(row["id"]),
                            Convert.ToString(row["name"]),
                            Convert.ToString(row["progress_category"]),
                            Convert.ToString(row["description"]),
                            Convert.ToString(row["badge_code"])
                        );

                        _skills.Add(skill.Id, skill);
                    }
                }

                // Load level requirements
                dbClient.SetQuery("SELECT * FROM `rp_skills_levels`");
                DataTable levelTable = dbClient.GetTable();

                if (levelTable != null)
                {
                    foreach (DataRow row in levelTable.Rows)
                    {
                        int skillId = Convert.ToInt32(row["skill_id"]);

                        if (!_skills.TryGetValue(skillId, out Skill skill))
                            continue;

                        int level = Convert.ToInt32(row["level"]);
                        int required = Convert.ToInt32(row["required_progress"]);

                        skill.Levels[level] = new SkillLevel(level, required);
                    }
                }
            }

            Log.Info($"[RP] Loaded {_skills.Count} Skills.");
        }

        public ICollection<Skill> GetSkills()
        {
            return _skills.Values;
        }

        public Skill GetSkillByProgressCategory(string progressCategory)
        {
            return _skills.Values.FirstOrDefault(x => x.ProgressCategory.Equals(progressCategory, StringComparison.OrdinalIgnoreCase));
        }

        public bool TryGetSkill(int skillId, out Skill skill)
        {
            return _skills.TryGetValue(skillId, out skill);
        }

        public Skill GetSkillById(int skillId)
        {
            _skills.TryGetValue(skillId, out var skill);
            return skill;
        }
    }
}
