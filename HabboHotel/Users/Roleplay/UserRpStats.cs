using System;

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
        public DateTime NextHungerTick { get; set; }

        public UserRpStats(int userId, int health, int shield, int energy, int hunger, int experience, int knockouts, int deaths, int arrested, int escapes, int damageDealt, int damageTaken, int attacks, int attacked)
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

            // not saved in db
            NextHungerTick = DateTime.UtcNow.AddMinutes(10);
        }
    }
}