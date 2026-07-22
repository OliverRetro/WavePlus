using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.Roleplay.Skill;

namespace Plus.HabboHotel.Permissions
{
    public sealed class SkillManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SkillManager));

        private readonly Dictionary<int, Skill> _skills = new();

        public void Init()
        {
            _skills.Clear();

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                // Load skills
                foreach (var row in db.RpSkills.AsNoTracking().ToList()) {
                    var skill = new Skill(
                        row.Id,
                        row.Name,
                        row.ProgressCategory,
                        row.Description,
                        row.BadgeCode
                    );

                    _skills.Add(skill.Id, skill);
                }

                // Load level requirements
                foreach (var row in db.RpSkillsLevels.AsNoTracking().ToList()) {
                    int skillId = row.SkillId;

                    if (!_skills.TryGetValue(skillId, out Skill skill))
                        continue;

                    int level = Convert.ToInt32(row.Level);
                    int required = row.RequiredProgress;

                    skill.Levels[level] = new SkillLevel(level, required);
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