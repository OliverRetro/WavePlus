using System;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using Plus.HabboHotel.Rooms;

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
        public double HospitalReleaseTime { get; set; }
        public double HospitalHealStartTime { get; set; }
        public int HospitalHealthStart { get; set; }
        public double NextPassiveHospitalHealTick { get; set; }
        public UserRpWeapons Weapons { get; set; }
        public int Aggression { get; set; }
        public bool PassiveMode { get; set; }
        public DateTime NextHungerTick { get; set; }

        public UserRpStats(int userId, int health, int shield, int energy, int hunger, int experience, int knockouts, int deaths, int arrested, int escapes, int damageDealt, int damageTaken, int attacks, int attacked, int knowledge, int strength, bool isDead, double hospitalReleaseTime, double hospitalHealStartTime, int hospitalHealthStart, int aggression, bool passiveMode)
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
            IsDead = isDead;
            HospitalReleaseTime = hospitalReleaseTime;
            HospitalHealStartTime = hospitalHealStartTime;
            HospitalHealthStart = hospitalHealthStart;
            NextPassiveHospitalHealTick = 0;
            Aggression = aggression;
            PassiveMode = passiveMode;

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

        public int GetMaxHunger(Habbo habbo)
        {
            if (habbo?.GetRpSkills() == null)
            {
                return 25;
            }

            return habbo.GetRpSkills().GetLevel(7, true) switch
            {
                1 => 50,
                2 => 75,
                3 => 100,
                _ => 25
            };
        }

        public int GetMaxEnergy(Habbo habbo)
        {
            if (habbo?.GetRpSkills() == null)
            {
                return 100;
            }

            return habbo.GetRpSkills().GetLevel(7, true) switch
            {
                1 => 110,
                2 => 130,
                3 => 150,
                _ => 100
            };
        }

        public void Heal(Habbo habbo, int health)
        {
            Health = Math.Min(Health + health, GetMaxHealth(habbo));
        }

        public void Kill(Habbo target, RoomUser user)
        {
            if (target == null || user == null) { return; }

            PlusEnvironment.GetHospitalManager().HandleDeath(this, target, user);
        }

        public UserWeapon ActiveWeapon()
        {
            return Weapons?.GetActiveWeapon();
        }
    }
}
