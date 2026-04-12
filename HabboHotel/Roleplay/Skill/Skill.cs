using System.Collections.Generic;

namespace Plus.HabboHotel.Roleplay.Skill
{
    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ProgressCategory { get; set; }
        public string Description { get; set; }
        public string BadgeCode { get; set; }

        public Dictionary<int, SkillLevel> Levels { get; }

        public Skill(int id, string name, string progressCategory, string description, string badgeCode)
        {
            Id = id;
            Name = name;
            ProgressCategory = progressCategory;
            Description = description;
            BadgeCode = badgeCode;

            Levels = new Dictionary<int, SkillLevel>();
        }
    }
}