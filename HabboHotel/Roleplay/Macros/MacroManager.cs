using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Roleplay.Macros
{
    /// <summary>
    /// Loads and persists user macro sets (keybind -> command), backed by `users_macros`.
    /// The selected macro per user is tracked in memory.
    /// </summary>
    public class MacroManager
    {
        // Each user always has exactly this many keybind sets ("tabs").
        private const int DefaultSetCount = 3;

        // userId -> (macroId -> Macro)
        private readonly ConcurrentDictionary<int, Dictionary<int, Macro>> _cache = new();
        // userId -> selected macro id
        private readonly ConcurrentDictionary<int, int> _selected = new();
        // userId -> whether macros are globally enabled (all sets toggled off when false)
        private readonly ConcurrentDictionary<int, bool> _enabled = new();

        public Dictionary<int, Macro> GetUserMacros(int userId)
        {
            return _cache.GetOrAdd(userId, LoadFromDatabase);
        }

        /// <summary>
        /// Guarantees the user has the three default keybind sets, creating any that are missing.
        /// </summary>
        public void EnsureDefaultSets(int userId)
        {
            Dictionary<int, Macro> macros = GetUserMacros(userId);
            while (macros.Count < DefaultSetCount)
                CreateMacro(userId, "Set " + (macros.Count + 1));
        }

        // Lazily loads the persisted enabled flag the first time it's asked for.
        public bool IsEnabled(int userId) => _enabled.GetOrAdd(userId, LoadEnabled);

        public void SetEnabled(int userId, bool enabled)
        {
            _enabled[userId] = enabled;

            using IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
            dbClient.SetQuery("INSERT INTO `users_macro_settings` (`user_id`, `enabled`) VALUES (@uid, @enabled) ON DUPLICATE KEY UPDATE `enabled` = @enabled");
            dbClient.AddParameter("uid", userId);
            dbClient.AddParameter("enabled", enabled ? "1" : "0");
            dbClient.RunQuery();
        }

        private bool LoadEnabled(int userId)
        {
            using IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
            dbClient.SetQuery("SELECT `enabled` FROM `users_macro_settings` WHERE `user_id` = @uid LIMIT 1");
            dbClient.AddParameter("uid", userId);
            return dbClient.GetInteger() == 1;
        }

        public Macro GetSelected(int userId)
        {
            Dictionary<int, Macro> macros = GetUserMacros(userId);
            if (macros.Count == 0)
                return null;

            if (_selected.TryGetValue(userId, out int id) && macros.TryGetValue(id, out Macro selected))
                return selected;

            return macros.Values.First();
        }

        public void SetSelected(int userId, int macroId)
        {
            if (GetUserMacros(userId).ContainsKey(macroId))
                _selected[userId] = macroId;
        }

        public Macro CreateMacro(int userId, string name)
        {
            Dictionary<int, Macro> macros = GetUserMacros(userId);
            int id;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `users_macros` (`user_id`, `name`, `configs`) VALUES (@uid, @name, @configs)");
                dbClient.AddParameter("uid", userId);
                dbClient.AddParameter("name", name);
                dbClient.AddParameter("configs", "{}");
                id = Convert.ToInt32(dbClient.InsertQuery());
            }

            Macro macro = new(id, userId, name);
            macros[id] = macro;
            return macro;
        }

        public void SaveMacro(int userId, int macroId, string name, Dictionary<string, string> sets)
        {
            Dictionary<int, Macro> macros = GetUserMacros(userId);
            if (!macros.TryGetValue(macroId, out Macro macro))
                return;

            macro.Name = name ?? macro.Name;
            macro.Sets.Clear();
            if (sets != null)
                foreach (var kvp in sets)
                    macro.Sets[kvp.Key] = kvp.Value;

            using IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
            dbClient.SetQuery("UPDATE `users_macros` SET `name` = @name, `configs` = @configs WHERE `id` = @id AND `user_id` = @uid LIMIT 1");
            dbClient.AddParameter("name", macro.Name);
            dbClient.AddParameter("configs", JsonSerializer.Serialize(macro.Sets));
            dbClient.AddParameter("id", macroId);
            dbClient.AddParameter("uid", userId);
            dbClient.RunQuery();
        }

        public void DeleteMacro(int userId, int macroId)
        {
            Dictionary<int, Macro> macros = GetUserMacros(userId);
            if (!macros.Remove(macroId))
                return;

            if (_selected.TryGetValue(userId, out int sel) && sel == macroId)
                _selected.TryRemove(userId, out _);

            using IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
            dbClient.SetQuery("DELETE FROM `users_macros` WHERE `id` = @id AND `user_id` = @uid LIMIT 1");
            dbClient.AddParameter("id", macroId);
            dbClient.AddParameter("uid", userId);
            dbClient.RunQuery();
        }

        public void Unload(int userId)
        {
            _cache.TryRemove(userId, out _);
            _selected.TryRemove(userId, out _);
            _enabled.TryRemove(userId, out _);
        }

        private Dictionary<int, Macro> LoadFromDatabase(int userId)
        {
            Dictionary<int, Macro> macros = new();
            using IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
            dbClient.SetQuery("SELECT `id`, `user_id`, `name`, `configs` FROM `users_macros` WHERE `user_id` = @uid");
            dbClient.AddParameter("uid", userId);
            DataTable table = dbClient.GetTable();
            if (table != null)
            {
                foreach (DataRow row in table.Rows)
                {
                    int id = Convert.ToInt32(row["id"]);
                    Dictionary<string, string> sets = ParseConfigs(Convert.ToString(row["configs"]));
                    macros[id] = new Macro(id, userId, Convert.ToString(row["name"]), sets);
                }
            }

            return macros;
        }

        private static Dictionary<string, string> ParseConfigs(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new Dictionary<string, string>();

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }
    }
}
