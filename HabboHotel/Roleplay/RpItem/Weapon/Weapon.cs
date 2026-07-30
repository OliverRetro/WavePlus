using System;

namespace Plus.HabboHotel.Roleplay.RpItem.Weapon
{
    public class Weapon
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string HitMessage { get; set; }
        public string CriticalHitMessage { get; set; }
        public int MinimumDamage { get; set; }
        public int MaximumDamage { get; set; }
        public int CriticalChance { get; set; }
        public double CriticalMultiplier { get; set; }
        public int Effect { get; set; }

        public int Range { get; set; }

        public bool AllowDiagonal { get; set; }

        public int Durability { get; set; }
        public int StunChance { get; set; }
        public string Image { get; set; }
        public int Rarity { get; set; }

        public Weapon(int id, string name, string description, string hitMessage, string criticalHitMessage, int minimumDamage, int maximumDamage, int criticalChance, double criticalMultiplier, int effect, int range, int durability, int stunChance, string image, int rarity = 0, bool allowDiagonal = true)
        {
            Rarity = rarity;
            Id = id;
            Name = name;
            Description = description;
            HitMessage = hitMessage;
            CriticalHitMessage = criticalHitMessage;
            MinimumDamage = minimumDamage;
            MaximumDamage = maximumDamage;
            CriticalChance = criticalChance;
            CriticalMultiplier = criticalMultiplier;
            Effect = effect;
            // 0 is a real setting — "must be standing on the same tile" — not a missing value.
            Range = Math.Max(0, range);
            AllowDiagonal = allowDiagonal;
            Durability = durability;
            StunChance = stunChance;
            Image = image;
        }
    }
}