using System;
using System.Collections.Generic;
using log4net;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Plus.Core
{
    public class RedisManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RedisManager));

        private ConnectionMultiplexer _connection;
        private IDatabase _database;

        private static readonly TimeSpan BreakerCooldown = TimeSpan.FromSeconds(30);
        private const int FailureThreshold = 3;
        private volatile bool _healthy = true;
        private int _consecutiveFailures;
        private DateTime _nextProbeUtc = DateTime.MinValue;
        private readonly object _breakerLock = new();

        public bool IsConnected => _connection != null && _connection.IsConnected;

        public bool IsAvailable => _database != null && Ready();

        private bool Ready()
        {
            if (_database == null)
                return false;
            if (_healthy)
                return true;

            lock (_breakerLock)
                return DateTime.UtcNow >= _nextProbeUtc;
        }

        private void TripBreaker(Exception ex)
        {
            bool justTripped = false;
            lock (_breakerLock) {
                _consecutiveFailures++;

                if (_healthy && _consecutiveFailures < FailureThreshold)
                    return;

                if (_healthy) {
                    _healthy = false;
                    justTripped = true;
                }
                _nextProbeUtc = DateTime.UtcNow.Add(BreakerCooldown);
            }

            if (justTripped) {
                Log.Error($"Redis went unavailable after {FailureThreshold} consecutive failures — degrading to database-only for {BreakerCooldown.TotalSeconds:0}s. ({ex.Message})");
                NotifyStaff("[REDIS] Cache is unavailable — running on the database failsafe. Play continues; performance may dip until it recovers.");
            }
        }

        private void MarkHealthy()
        {
            bool wasUnhealthy;
            lock (_breakerLock) {
                _consecutiveFailures = 0;
                wasUnhealthy = !_healthy;
                _healthy = true;
            }

            if (wasUnhealthy) {
                Log.Info("Redis recovered — cache is back online.");
                NotifyStaff("[REDIS] Cache recovered and is back online.");
            }
        }

        private static void NotifyStaff(string message)
        {
            try {
                PlusEnvironment.GetGame()?.GetClientManager()?.ModAlert(message);
            } catch {
                // Never let the failsafe notification become a new point of failure.
            }
        }

        public void Init()
        {
            ConfigurationData config = PlusEnvironment.GetConfig();

            if (!TryGetBool(config, "redis.enabled", true)) {
                Log.Info("Redis is disabled in the configuration.");
                return;
            }

            string hostname = GetValue(config, "redis.hostname", "127.0.0.1");
            int port = TryGetInt(config, "redis.port", 6379);
            string password = GetValue(config, "redis.password", "");
            int database = TryGetInt(config, "redis.database", 0);

            try {
                var options = new ConfigurationOptions
                {
                    EndPoints = { { hostname, port } },
                    AbortOnConnectFail = false,
                    ConnectTimeout = 2000,
                    // Keep synchronous ops short so a Redis stall can't hang the game loop
                    SyncTimeout = 5000,
                    ConnectRetry = 3,
                    KeepAlive = 5,
                    DefaultDatabase = database
                };

                if (!string.IsNullOrEmpty(password))
                    options.Password = password;

                _connection = ConnectionMultiplexer.Connect(options);
                _database = _connection.GetDatabase(database);

                if (IsConnected)
                    Log.Info($"Connected to Redis at {hostname}:{port} (db {database})!");
                else
                    Log.Warn($"Could not connect to Redis at {hostname}:{port}.");
            } catch (Exception ex) {
                Log.Error($"Failed to initialise Redis: {ex.Message}");
                _connection = null;
                _database = null;
            }
        }

        public IDatabase GetDatabase()
        {
            return Ready() ? _database : null;
        }

        public void ListPush(string key, string value)
        {
            if (!Ready())
                return;

            try {
                _database.ListRightPush(key, value);
                MarkHealthy();
            } catch (Exception ex) {
                TripBreaker(ex);
            }
        }

        public void ListPushFireAndForget(string key, string value)
        {
            if (!Ready())
                return;

            try {
                _database.ListRightPush(key, value, flags: CommandFlags.FireAndForget);
                MarkHealthy();
            } catch (Exception ex) {
                TripBreaker(ex);
            }
        }

        public bool SetObject<T>(string key, T value, TimeSpan? expiry = null)
        {
            if (!Ready())
                return false;

            try {
                string json = JsonConvert.SerializeObject(value);
                bool result = _database.StringSet(key, json, expiry);
                MarkHealthy();
                return result;
            } catch (Exception ex) {
                TripBreaker(ex);
                return false;
            }
        }

        public T GetObject<T>(string key)
        {
            if (!Ready())
                return default;

            try {
                RedisValue value = _database.StringGet(key);
                MarkHealthy();
                if (value.IsNullOrEmpty)
                    return default;

                return JsonConvert.DeserializeObject<T>(value);
            } catch (Exception ex) {
                TripBreaker(ex);
                return default;
            }
        }

        public bool KeyExists(string key)
        {
            if (!Ready())
                return false;

            try {
                bool result = _database.KeyExists(key);
                MarkHealthy();
                return result;
            } catch (Exception ex) {
                TripBreaker(ex);
                return false;
            }
        }

        public void Delete(string key)
        {
            if (!Ready())
                return;

            try {
                _database.KeyDelete(key);
                MarkHealthy();
            } catch (Exception ex) {
                TripBreaker(ex);
            }
        }

        public void SetAdd(string setKey, string member)
        {
            if (!Ready())
                return;

            try {
                _database.SetAdd(setKey, member);
                MarkHealthy();
            } catch (Exception ex) {
                TripBreaker(ex);
            }
        }

        public void SetRemove(string setKey, string member)
        {
            if (!Ready())
                return;

            try {
                _database.SetRemove(setKey, member);
                MarkHealthy();
            } catch (Exception ex) {
                TripBreaker(ex);
            }
        }

        public List<string> SetMembers(string setKey)
        {
            var result = new List<string>();
            if (!Ready())
                return result;

            try {
                foreach (RedisValue member in _database.SetMembers(setKey))
                    result.Add(member);
                MarkHealthy();
            } catch (Exception ex) {
                TripBreaker(ex);
            }

            return result;
        }

        public void Dispose()
        {
            _connection?.Dispose();
            _connection = null;
            _database = null;
        }

        private static string GetValue(ConfigurationData config, string key, string fallback)
        {
            return config.Data.TryGetValue(key, out string value) ? value : fallback;
        }

        private static int TryGetInt(ConfigurationData config, string key, int fallback)
        {
            return config.Data.TryGetValue(key, out string value) && int.TryParse(value, out int parsed)
                ? parsed
                : fallback;
        }

        private static bool TryGetBool(ConfigurationData config, string key, bool fallback)
        {
            if (!config.Data.TryGetValue(key, out string value))
                return fallback;

            return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
    }
}