using System;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;

namespace Plus.HabboHotel.Users
{
    public class UserRpStats
    {
        public int UserId { get; set; }
        public int Health { get; set; }
        public int Shield { get; set; }
        public int Energy { get; set; }
        public int Hunger { get; set; }
        public int Experience { get; set; }
        public int Knockouts { get; set; }
        public int Deaths { get; set; }
        public int Arrested { get; set; }
        public int Escapes { get; set; }
        public int DamageDealt { get; set; }
        public int DamageTaken { get; set; }
        public int Attacks { get; set; }
        public int Attacked { get; set; }
        public int Knowledge { get; set; }
        public int Strength { get; set; }
        public bool IsDead { get; set; }
        public DateTime NextHungerTick { get; set; }
        public UserRpWeapons Weapons { get; set; }

        public UserRpStats(int userId, int health, int shield, int energy, int hunger, int experience, int knockouts, int deaths, int arrested, int escapes, int damageDealt, int damageTaken, int attacks, int attacked, int knowledge, int strength)
        {
            UserId = userId;
            Health = health;
            Shield = shield;
            Energy = energy;
            Hunger = hunger;
            Experience = experience;
            Knockouts = knockouts;
            Deaths = deaths;
            Arrested = arrested;
            Escapes = escapes;
            DamageDealt = damageDealt;
            DamageTaken = damageTaken;
            Attacks = attacks;
            Attacked = attacked;
            Knowledge = knowledge;
            Strength = strength;

            // not saved in db
            IsDead = false;
            NextHungerTick = DateTime.UtcNow.AddMinutes(10);
        }

        public int GetMaxHealth(Habbo habbo)
        {
            if (habbo?.GetRpSkills() == null)
            {
                return 100;
            }

            return habbo.GetRpSkills().GetLevel(7, true) switch
            {
                1 => 125,
                2 => 150,
                3 => 175,
                _ => 100
            };
        }

        public void Heal(Habbo habbo, int health)
        {
            IsDead = false;
            Health = Math.Min(Health + health, GetMaxHealth(habbo));
        }

        public void Kill(Habbo target)
        {
            // for combat
            target.GetRpStats().IsDead = true;
            target.GetRpStats().Deaths++;
            Knockouts++;
        }

        public UserWeapon ActiveWeapon()
        {
            return Weapons?.GetActiveWeapon();
        }
    }
}
