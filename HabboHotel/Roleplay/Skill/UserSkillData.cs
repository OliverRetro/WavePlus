namespace Plus.HabboHotel.Roleplay.Skill
{
    public class UserSkillData
    {
        public int SkillId { get; }
        public int Progress { get; private set; }
        public bool Equipped { get; private set; }

        public UserSkillData(int skillId, int progress, bool equipped)
        {
            SkillId = skillId;
            Progress = progress;
            Equipped = equipped;
        }

        public void AddProgress(int progress)
        {
            if (progress <= 0)
                return;

            Progress += progress;
        }

        public void SetProgress(int progress)
        {
            Progress = progress;
        }

        public void SetEquipped(bool equipped)
        {
            Equipped = equipped;
        }
    }
}