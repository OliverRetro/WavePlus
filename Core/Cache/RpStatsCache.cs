using System.Linq;
using Plus.HabboHotel.Roleplay.RpItem.Item;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Roleplay;

namespace Plus.Core.Cache
{
    // Cache & mirror the data for Redis usage
    public static class RpStatsCache
    {
        public const string ActiveSetKey = "rp:users:active";

        private static string StatsKey(int userId) => $"rp:user:{userId}:stats";
        private static string HabboStatsKey(int userId) => $"user:{userId}:stats";
        private static string ItemsKey(int userId) => $"rp:user:{userId}:items";
        private static string WeaponsKey(int userId) => $"rp:user:{userId}:weapons";

        private sealed class Snapshot
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
            public int Aggression { get; set; }
            public bool PassiveMode { get; set; }
            public int Robberies { get; set; }
            public int Robbed { get; set; }
            public bool IsCuffed { get; set; }
            public int JailStars { get; set; }
        }

        public static void Mirror(Habbo habbo)
        {
            UserRpStats stats = habbo?.GetRpStats();
            if (stats == null)
                return;

            RedisManager redis = PlusEnvironment.GetRedis();
            if (redis == null || !redis.IsAvailable)
                return;

            var snapshot = new Snapshot
            {
                UserId = habbo.Id,
                Health = stats.Health,
                Shield = stats.Shield,
                Energy = stats.Energy,
                Hunger = stats.Hunger,
                Experience = stats.Experience,
                Knockouts = stats.Knockouts,
                Deaths = stats.Deaths,
                Arrested = stats.Arrested,
                Escapes = stats.Escapes,
                DamageDealt = stats.DamageDealt,
                DamageTaken = stats.DamageTaken,
                Attacks = stats.Attacks,
                Attacked = stats.Attacked,
                Knowledge = stats.Knowledge,
                Strength = stats.Strength,
                IsDead = stats.IsDead,
                Aggression = stats.Aggression,
                PassiveMode = stats.PassiveMode,
                Robberies = stats.Robberies,
                Robbed = stats.Robbed,
                IsCuffed = stats.IsCuffed,
                JailStars = stats.JailStars
            };

            redis.SetObject(StatsKey(habbo.Id), snapshot);
            redis.SetAdd(ActiveSetKey, habbo.Id.ToString());
        }

        public static void MirrorAll(Habbo habbo)
        {
            if (habbo == null)
                return;

            RedisManager redis = PlusEnvironment.GetRedis();
            if (redis == null || !redis.IsAvailable)
                return;

            Mirror(habbo);
            MirrorHabboStats(habbo, redis);
            MirrorRpItems(habbo, redis);
            MirrorRpWeapons(habbo, redis);
        }

        private static void MirrorHabboStats(Habbo habbo, RedisManager redis)
        {
            HabboStats stats = habbo.GetStats();
            if (stats == null)
                return;

            redis.SetObject(HabboStatsKey(habbo.Id), new
            {
                UserId = habbo.Id,
                habbo.Username,
                habbo.Credits,
                habbo.Duckets,
                habbo.Diamonds,
                habbo.GotwPoints,
                stats.Respect,
                stats.RespectGiven,
                stats.DailyRespectPoints,
                stats.AchievementPoints,
                stats.RoomVisits,
                stats.OnlineTime,
                stats.QuestId,
                stats.FavouriteGroupId
            });
        }

        private static void MirrorRpItems(Habbo habbo, RedisManager redis)
        {
            UserRpItems items = habbo.GetRpItems();
            if (items == null)
                return;

            redis.SetObject(ItemsKey(habbo.Id), items.Items.Select(i => new
            {
                i.Id,
                i.ItemId,
                Name = i.ItemData?.Name,
                i.Uses,
                i.Slot,
                i.DurabilityLeft,
                i.Equipped
            }).ToList());
        }

        private static void MirrorRpWeapons(Habbo habbo, RedisManager redis)
        {
            UserRpWeapons weapons = habbo.GetRpWeapons();
            if (weapons == null)
                return;

            redis.SetObject(WeaponsKey(habbo.Id), new
            {
                weapons.ActiveWeaponId,
                Weapons = weapons.Weapons.Select(w => new
                {
                    w.Id,
                    w.WeaponId,
                    Name = w.WeaponData?.Name,
                    w.DurabilityLeft,
                    w.Slot,
                    w.IsDefault
                }).ToList()
            });
        }

        public static void Remove(int userId)
        {
            RedisManager redis = PlusEnvironment.GetRedis();
            if (redis == null || !redis.IsAvailable)
                return;

            redis.Delete(StatsKey(userId));
            redis.Delete(HabboStatsKey(userId));
            redis.Delete(ItemsKey(userId));
            redis.Delete(WeaponsKey(userId));
            redis.SetRemove(ActiveSetKey, userId.ToString());
        }
    }
}