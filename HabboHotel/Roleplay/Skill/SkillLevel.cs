using System;
using System.Collections.Generic;
using System.Text;

namespace Plus.HabboHotel.Roleplay.Skill
{
    public class SkillLevel
    {
        public int Level { get; }
        public int RequiredProgress { get; }

        public SkillLevel(int level, int requiredProgress)
        {
            Level = level;
            RequiredProgress = requiredProgress;
        }
    }
}
