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

        public bool IsConnected => _connection != null && _connection.IsConnected;

        /// <summary>True when Redis is available for the write-behind cache to use.</summary>
        public bool IsAvailable => _database != null;

        public void Init()
        {
            ConfigurationData config = PlusEnvironment.GetConfig();

            if (!TryGetBool(config, "redis.enabled", true))
            {
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

        /// <summary>
        /// Stores an object as JSON. JSON keeps the cached value readable via redis-cli
        /// (GET key) while still round-tripping to strongly-typed C#. Returns false when
        /// Redis is unavailable so callers can fall back to the database.
        /// </summary>
        public bool SetObject<T>(string key, T value, TimeSpan? expiry = null)
        {
            if (_database == null)
                return false;

            try
            {
                string json = JsonConvert.SerializeObject(value);
                return _database.StringSet(key, json, expiry);
            }
            catch (Exception ex)
            {
                Log.Error($"Redis SetObject({key}) failed: {ex.Message}");
                return false;
            }
        }

        public T GetObject<T>(string key)
        {
            if (_database == null)
                return default;

            try
            {
                RedisValue value = _database.StringGet(key);
                if (value.IsNullOrEmpty)
                    return default;

                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception ex)
            {
                Log.Error($"Redis GetObject({key}) failed: {ex.Message}");
                return default;
            }
        }

        public bool KeyExists(string key)
        {
            if (_database == null)
                return false;

            try
            {
                return _database.KeyExists(key);
            }
            catch (Exception ex)
            {
                Log.Error($"Redis KeyExists({key}) failed: {ex.Message}");
                return false;
            }
        }

        public void Delete(string key)
        {
            if (_database == null)
                return;

            try
            {
                _database.KeyDelete(key);
            }
            catch (Exception ex)
            {
                Log.Error($"Redis Delete({key}) failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a member to an index set (e.g. the set of user ids currently cached).
        /// Periodic savers iterate these sets instead of SCAN-ing the whole keyspace.
        /// </summary>
        public void SetAdd(string setKey, string member)
        {
            if (_database == null)
                return;

            try
            {
                _database.SetAdd(setKey, member);
            }
            catch (Exception ex)
            {
                Log.Error($"Redis SetAdd({setKey}) failed: {ex.Message}");
            }
        }

        public void SetRemove(string setKey, string member)
        {
            if (_database == null)
                return;

            try
            {
                _database.SetRemove(setKey, member);
            }
            catch (Exception ex)
            {
                Log.Error($"Redis SetRemove({setKey}) failed: {ex.Message}");
            }
        }

        public List<string> SetMembers(string setKey)
        {
            var result = new List<string>();
            if (_database == null)
                return result;

            try
            {
                foreach (RedisValue member in _database.SetMembers(setKey))
                    result.Add(member);
            }
            catch (Exception ex)
            {
                Log.Error($"Redis SetMembers({setKey}) failed: {ex.Message}");
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
