using System.Collections.Generic;

namespace Plus.HabboHotel.Roleplay.RpItem.Weapon
{
    public class Weapon
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int MinimumDamage { get; set; }
        public int MaximumDamage { get; set; }
        public int CriticalChance { get; set; }
        public double CriticalMultiplier { get; set; }
        public int Effect { get; set; }
        public int Range { get; set; }
        public int Durability { get; set; }

        public Weapon(int id, string name, string description, int minimumDamage, int maximumDamage, int criticalChance, double criticalMultiplier, int effect, int range, int durability)
        {
            Id = id;
            Name = name;
            Description = description;
            MinimumDamage = minimumDamage;
            MaximumDamage = maximumDamage;
            CriticalChance = criticalChance;
            CriticalMultiplier = criticalMultiplier;
            Effect = effect;
            Range = range;
            Durability = durability;
        }
    }
}