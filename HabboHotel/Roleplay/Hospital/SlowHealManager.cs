using System;
using System.Collections.Concurrent;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Roleplay;

namespace Plus.HabboHotel.Roleplay.Hospital
{
    public sealed class SlowHealManager
    {
        // timer between heal (+1hp)
        public const double DripIntervalSeconds = 0.5;

        private sealed class ActiveHeal
        {
            public int Remaining;
            public int ExpectedHealth;
            public int ExpectedShield;
            public double NextDripUnix;
        }

        private readonly ConcurrentDictionary<int, ActiveHeal> _active = new();

        public void Begin(Habbo habbo, int hpAmount)
        {
            UserRpStats stats = habbo?.GetRpStats();
            if (habbo == null || stats == null || hpAmount <= 0)
                return;

            int headroom = UserRpStats.GetMaxHealth(habbo) - stats.Health;
            if (headroom <= 0)
                return;

            _active[habbo.Id] = new ActiveHeal
            {
                Remaining = Math.Min(hpAmount, headroom),
                ExpectedHealth = stats.Health,
                ExpectedShield = stats.Shield,
                NextDripUnix = PlusEnvironment.GetUnixTimestampPrecise() + DripIntervalSeconds
            };
        }

        public void Cancel(int userId) => _active.TryRemove(userId, out _);

        public bool IsHealing(int userId) => _active.ContainsKey(userId);

        public void OnCycle()
        {
            if (_active.IsEmpty)
                return;

            double now = PlusEnvironment.GetUnixTimestampPrecise();

            foreach (var pair in _active) {
                ActiveHeal heal = pair.Value;

                GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(pair.Key);
                Habbo habbo = client?.GetHabbo();
                UserRpStats stats = habbo?.GetRpStats();

                if (habbo == null || stats == null || stats.IsDead) {
                    _active.TryRemove(pair.Key, out _);
                    continue;
                }

                // no refunds take the L
                if (stats.Health < heal.ExpectedHealth || stats.Shield < heal.ExpectedShield) {
                    _active.TryRemove(pair.Key, out _);
                    client.SendWhisper("Your treatment was interrupted!", 1);
                    continue;
                }

                if (now < heal.NextDripUnix)
                    continue;

                int maxHealth = UserRpStats.GetMaxHealth(habbo);
                if (stats.Health >= maxHealth) {
                    _active.TryRemove(pair.Key, out _);
                    continue;
                }

                stats.Health = Math.Min(maxHealth, stats.Health + 1);
                heal.ExpectedHealth = stats.Health;
                heal.ExpectedShield = stats.Shield;
                heal.Remaining--;
                heal.NextDripUnix = now + DripIntervalSeconds;
                habbo.TrySendUserStatsUpdate(true);

                if (heal.Remaining <= 0 || stats.Health >= maxHealth)
                    _active.TryRemove(pair.Key, out _);
            }
        }
    }
}