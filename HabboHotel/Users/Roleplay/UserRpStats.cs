using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Rooms;
using System;

namespace Plus.HabboHotel.Users.Roleplay
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
        public int AttributePoints { get; set; }
        public bool IsDead { get; set; }
        public double HospitalReleaseTime { get; set; }
        public double HospitalHealStartTime { get; set; }
        public int HospitalHealthStart { get; set; }
        public double NextPassiveHospitalHealTick { get; set; }
        public UserRpWeapons Weapons { get; set; }
        public int Aggression { get; set; }
        public bool PassiveMode { get; set; }
        public int Robberies { get; set; }
        public int Robbed { get; set; }
        public bool IsCuffed { get; set; }
        public double JailReleaseTime { get; set; }
        public bool JailPending { get; set; }
        public int JailStars { get; set; }
        public string JailRevertLook { get; set; }
        public int JailRoomId { get; set; }
        public double JailTimeLeft { get; set; }
        public DateTime NextHungerTick { get; set; }
        public bool IsGodMode { get; set; }

        public UserRpStats(int userId, int health, int shield, int energy, int hunger, int experience, int knockouts, int deaths, int arrested, int escapes, int damageDealt, int damageTaken, int attacks, int attacked, int knowledge, int strength, bool isDead, double hospitalReleaseTime, double hospitalHealStartTime, int hospitalHealthStart, int aggression, bool passiveMode, int robberies, int robbed, bool isCuffed = false, double jailReleaseTime = 0, bool jailPending = false, int jailStars = 0, string jailRevertLook = "", int jailRoomId = 0, double jailTimeLeft = 0)
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
            Robberies = robberies;
            Robbed = robbed;
            IsCuffed = isCuffed;
            JailReleaseTime = jailReleaseTime;
            JailPending = jailPending;
            JailStars = jailStars;
            JailRevertLook = jailRevertLook ?? "";
            JailRoomId = jailRoomId;
            JailTimeLeft = jailTimeLeft;

            IsGodMode = false;

            NextHungerTick = DateTime.UtcNow.AddMinutes(10);
        }

        public const int MaxAttributePoints = 10;

        public static int GetAttributeBonus(int points)
        {
            points = Math.Clamp(points, 0, MaxAttributePoints);
            return Math.Min(points, 5) + (Math.Max(0, points - 5) * 2);
        }

        public int GetAvailablePoints() => Math.Max(0, Math.Min(AttributePoints, MaxAttributePoints - (Strength + Knowledge)));

        public static int GetMaxHealth(Habbo habbo)
        {
            if (habbo?.GetRpSkills() == null) {
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

        public static int GetMaxHunger(Habbo habbo)
        {
            if (habbo?.GetRpSkills() == null) {
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

        public static int GetMaxEnergy(Habbo habbo)
        {
            if (habbo?.GetRpSkills() == null) {
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

        public static void Kill(Habbo victim, RoomUser victimUser, UserRpStats attacker = null)
        {
            if (victim == null || victimUser == null) { return; }

            PlusEnvironment.GetHospitalManager().HandleDeath(victim, victimUser, attacker);
        }

        public void TickHunger(Habbo habbo)
        {
            if (DateTime.UtcNow < NextHungerTick)
                return;

            // Idle players don't burn hunger — you only get hungry while active. Push the next
            // tick out a little so it resumes promptly once they wake up.
            RoomUser idleCheck = habbo.CurrentRoom?.GetRoomUserManager()?.GetRoomUserByHabbo(habbo.Id);
            if (idleCheck != null && idleCheck.IsAsleep || habbo.GetRpStats().IsGodMode) {
                NextHungerTick = DateTime.UtcNow.AddMinutes(1);
                return;
            }

            if (Hunger > 0) {
                Hunger = Math.Max(0, Hunger -= 2);
                NextHungerTick = DateTime.UtcNow.AddMinutes(10);
                if (habbo.GetRpStats().Hunger <= 10) {
                    habbo.GetClient().SendWhisper($"You lost a total of -2 hunger - consume some food to regain it! [{habbo.GetRpStats().Hunger}/{GetMaxHunger(habbo)}]", 1);
                }
            } else {
                int damage = Random.Shared.Next(5, 51);
                Health = Math.Max(0, Health - damage);
                NextHungerTick = DateTime.UtcNow.AddMinutes(1);

                if (Health <= 0 && !IsDead) {
                    Room room = habbo.CurrentRoom;
                    RoomUser roomUser = room?.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
                    if (roomUser != null)
                        LiveFeedService.LiveFeed("💀 " + LiveFeedService.Name(habbo.Username, "green") + " died of hunger.", "red");
                    Kill(habbo, roomUser);
                }
            }

            habbo.TrySendUserStatsUpdate(true);
        }

        public void InteractionEnergy()
        {
            int energyLoss = Random.Shared.Next(1, 6); // 1-5 dmg
            Energy = Math.Max(0, Energy - energyLoss);
        }

        public static int GetRobberyChance(Habbo habbo)
        {
            if (habbo?.GetRpSkills() == null) {
                return 50;
            }

            return habbo.GetRpSkills().GetLevel(3, true) switch
            {
                1 => 52,
                2 => 55,
                3 => 60,
                _ => 50
            };
        }

        public UserWeapon ActiveWeapon()
        {
            return Weapons?.GetActiveWeapon();
        }

        public static int GetScavengerBoost(Habbo habbo)
        {
            if (habbo?.GetRpSkills() == null) {
                return 1;
            }

            return habbo.GetRpSkills().GetLevel(8, true) switch
            {
                1 => 5,
                2 => 10,
                3 => 15,
                _ => 1
            };
        }
    }
}