using System;
using System.Collections.Concurrent;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Roleplay.Cooldowns
{
    public sealed class RpCooldownManager
    {
        // Rp items have a unique random cooldown, or not.
        private readonly ConcurrentDictionary<(int UserId, RpCooldownKind Kind, int Scope), double> _cooldowns = new();

        private double _nextPurgeUnix;

        private const double PurgeIntervalSeconds = 60;

        public static double GetDefaultSeconds(RpCooldownKind kind) => kind switch
        {
            RpCooldownKind.Command => GetSettingDouble("rp.cooldown.command.seconds", 1),
            RpCooldownKind.Combat => GetSettingDouble("rp.cooldown.combat.seconds", 3),
            RpCooldownKind.CombatWhiff => GetSettingDouble("rp.cooldown.combat.whiff.seconds", 1),
            RpCooldownKind.Robbery => GetSettingDouble("rp.cooldown.robbery.seconds", 90),
            RpCooldownKind.RpVendor => GetSettingDouble("rp.cooldown.vendor.seconds", 5),
            RpCooldownKind.TrashSearch => GetSettingDouble("rp.trash.search.cooldown.seconds", 60),
            _ => 0
        };

        public bool IsReady(int userId, RpCooldownKind kind, int scope = 0) => RemainingSeconds(userId, kind, scope) <= 0;

        public double RemainingSeconds(int userId, RpCooldownKind kind, int scope = 0)
        {
            if (!_cooldowns.TryGetValue((userId, kind, scope), out double readyAt))
                return 0;

            double remaining = readyAt - PlusEnvironment.GetUnixTimestampPrecise();
            return remaining > 0 ? remaining : 0;
        }

        public int RemainingSecondsCeil(int userId, RpCooldownKind kind, int scope = 0) =>
            (int)Math.Ceiling(RemainingSeconds(userId, kind, scope));

        public void Apply(int userId, RpCooldownKind kind) => Apply(userId, kind, GetDefaultSeconds(kind));

        public void Apply(int userId, RpCooldownKind kind, double seconds, int scope = 0)
        {
            if (seconds <= 0) {
                Clear(userId, kind, scope);
                return;
            }

            _cooldowns[(userId, kind, scope)] = PlusEnvironment.GetUnixTimestampPrecise() + seconds;
        }

        public bool TryConsume(GameClient session, RpCooldownKind kind, string message = null, double? seconds = null, int scope = 0)
        {
            Habbo habbo = session?.GetHabbo();
            if (habbo == null)
                return false;

            if (!IsReady(habbo.Id, kind, scope)) {
                if (!string.IsNullOrEmpty(message))
                    session.SendWhisper(message.Replace("%seconds%", RemainingSecondsCeil(habbo.Id, kind, scope).ToString()), 1);

                return false;
            }

            Apply(habbo.Id, kind, seconds ?? GetDefaultSeconds(kind), scope);
            return true;
        }

        public void Clear(int userId, RpCooldownKind kind, int scope = 0) => _cooldowns.TryRemove((userId, kind, scope), out _);

        public void ClearAll(int userId)
        {
            foreach (var entry in _cooldowns) {
                if (entry.Key.UserId == userId)
                    _cooldowns.TryRemove(entry.Key, out _);
            }
        }

        public void OnCycle()
        {
            double now = PlusEnvironment.GetUnixTimestampPrecise();
            if (now < _nextPurgeUnix)
                return;

            _nextPurgeUnix = now + PurgeIntervalSeconds;

            foreach (var entry in _cooldowns) {
                if (entry.Value <= now)
                    _cooldowns.TryRemove(entry.Key, out _);
            }
        }

        private static double GetSettingDouble(string key, double fallback)
        {
            string value = PlusEnvironment.GetSettingsManager()?.TryGetValue(key);
            return double.TryParse(value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out double parsed) ? parsed : fallback;
        }
    }
}