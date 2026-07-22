using System;
using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.Permissions;

namespace Plus.HabboHotel.Roleplay.Skill
{
    public class UserRpSkills
    {
        public const int MaxEquippedSkills = 4;

        private readonly Dictionary<int, UserSkillData> _skills;
        private readonly SkillManager _skillManager;

        public UserRpSkills(SkillManager skillManager, IEnumerable<UserSkillData> skills)
        {
            _skillManager = skillManager;
            _skills = skills.ToDictionary(x => x.SkillId, x => x);
        }

        public IEnumerable<UserSkillData> Skills => _skills.Values;

        public bool Dirty { get; private set; }

        public UserSkillData GetOrCreateSkill(int skillId)
        {
            if (_skills.TryGetValue(skillId, out UserSkillData skill))
                return skill;

            skill = new UserSkillData(skillId, 0, false);
            _skills[skillId] = skill;
            Dirty = true;
            return skill;
        }

        public int GetProgress(int skillId)
        {
            return _skills.TryGetValue(skillId, out UserSkillData skill) ? skill.Progress : 0;
        }

        public bool HasSkill(int skillId)
        {
            return HasUnlockedSkill(skillId) && IsEquipped(skillId);
        }

        public bool HasUnlockedSkill(int skillId)
        {
            return GetLevel(skillId) > 0;
        }

        public int GetLevel(int skillId, bool equippedCheck = false)
        {
            if (!_skillManager.TryGetSkill(skillId, out Skill skill))
                return 0;

            if (equippedCheck && !IsEquipped(skillId))
                return 0;

            int progress = GetProgress(skillId);
            int highestLevel = 0;

            foreach (SkillLevel level in skill.Levels.Values.OrderBy(x => x.Level)) {
                if (progress < level.RequiredProgress)
                    break;

                highestLevel = level.Level;
            }

            return highestLevel;
        }

        public bool IsEquipped(int skillId)
        {
            return _skills.TryGetValue(skillId, out UserSkillData skill) && skill.Equipped;
        }

        public int EquippedCount()
        {
            return _skills.Values.Count(x => x.Equipped && GetLevel(x.SkillId) > 0);
        }

        public bool CanEquipMoreSkills()
        {
            return EquippedCount() < MaxEquippedSkills;
        }

        public int GetLevelByProgressCategory(string progressCategory)
        {
            if (string.IsNullOrWhiteSpace(progressCategory))
                return 0;

            Skill skill = _skillManager.GetSkillByProgressCategory(progressCategory);
            return skill == null ? 0 : GetLevel(skill.Id);
        }

        public int ProgressSkill(int skillId, int progress)
        {
            if (progress <= 0 || !_skillManager.TryGetSkill(skillId, out _))
                return GetLevel(skillId);

            GetOrCreateSkill(skillId).AddProgress(progress);
            Dirty = true;
            return GetLevel(skillId);
        }

        public int ProgressSkillByCategory(string progressCategory, int progress)
        {
            if (progress <= 0)
                return 0;

            Skill skill = _skillManager.GetSkillByProgressCategory(progressCategory);
            return skill == null ? 0 : ProgressSkill(skill.Id, progress);
        }

        public bool GrantMaxSkill(int skillId)
        {
            if (!_skillManager.TryGetSkill(skillId, out Skill skill) || skill.Levels.Count == 0)
                return false;

            int maxProgress = skill.Levels.Values.Max(x => x.RequiredProgress);
            UserSkillData userSkill = GetOrCreateSkill(skillId);
            userSkill.SetProgress(maxProgress);
            Dirty = true;
            return true;
        }

        public void SetEquipped(int skillId, bool equipped)
        {
            if (equipped && !HasUnlockedSkill(skillId))
                return;

            if (equipped && !IsEquipped(skillId) && !CanEquipMoreSkills())
                return;

            GetOrCreateSkill(skillId).SetEquipped(equipped);
            Dirty = true;
        }

        public bool ToggleSkill(int skillId)
        {
            if (!HasUnlockedSkill(skillId))
                return false;

            if (IsEquipped(skillId)) {
                SetEquipped(skillId, false);
                return true;
            }

            if (!CanEquipMoreSkills())
                return false;

            SetEquipped(skillId, true);
            return true;
        }

        public void MarkSaved()
        {
            Dirty = false;
        }
    }
}