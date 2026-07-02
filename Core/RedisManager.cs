using System;
using log4net;
using StackExchange.Redis;

namespace Plus.Core
{
    public class RedisManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RedisManager));

        private ConnectionMultiplexer _connection;
        private IDatabase _database;

        public bool IsConnected => _connection != null && _connection.IsConnected;

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

            try
            {
                var options = new ConfigurationOptions
                {
                    EndPoints = { { hostname, port } },
                    AbortOnConnectFail = false,
                    ConnectTimeout = 5000,
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
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to initialise Redis: {ex.Message}");
                _connection = null;
                _database = null;
            }
        }

        /// <summary>
        /// Returns the underlying database, or null when Redis is unavailable.
        /// </summary>
        public IDatabase GetDatabase()
        {
            return _database;
        }

        /// <summary>
        /// Appends a value to the tail of a Redis list. Fails silently when Redis is down.
        /// </summary>
        public void ListPush(string key, string value)
        {
            if (_database == null)
                return;

            try
            {
                _database.ListRightPush(key, value);
            }
            catch (Exception ex)
            {
                Log.Error($"Redis ListPush failed: {ex.Message}");
            }
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
