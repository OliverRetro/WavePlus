using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using System.Text.Json;

namespace Plus.HabboHotel.Roleplay.Macros
{
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

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            db.UsersMacroSettings.Upsert(new UsersMacroSettingEntity
            {
                UserId = userId,
                Enabled = enabled
            }).Run();
        }

        private bool LoadEnabled(int userId)
        {
            using WavePlusContext db = PlusEnvironment.GetDbContext();
            return db.UsersMacroSettings.Where(x => x.UserId == userId).Select(x => x.Enabled).FirstOrDefault();
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
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var entity = new Database.EF.Entities.UsersMacroEntity { UserId = userId, Name = name, Configs = "{}" };
                db.UsersMacros.Add(entity);
                db.SaveChanges();
                id = entity.Id;
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

            string macroName = macro.Name;
            string configs = JsonSerializer.Serialize(macro.Sets);
            using WavePlusContext db = PlusEnvironment.GetDbContext();
            db.UsersMacros.Where(m => m.Id == macroId && m.UserId == userId)
                .ExecuteUpdate(s => s.SetProperty(m => m.Name, macroName).SetProperty(m => m.Configs, configs));
        }

        public void DeleteMacro(int userId, int macroId)
        {
            Dictionary<int, Macro> macros = GetUserMacros(userId);
            if (!macros.Remove(macroId))
                return;

            if (_selected.TryGetValue(userId, out int sel) && sel == macroId)
                _selected.TryRemove(userId, out _);

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            db.UsersMacros.Where(m => m.Id == macroId && m.UserId == userId).ExecuteDelete();
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
            using WavePlusContext db = PlusEnvironment.GetDbContext();
            foreach (var row in db.UsersMacros.Where(m => m.UserId == userId).Select(m => new { m.Id, m.Name, m.Configs }).ToList()) {
                Dictionary<string, string> sets = ParseConfigs(row.Configs);
                macros[row.Id] = new Macro(row.Id, userId, row.Name, sets);
            }

            return macros;
        }

        private static Dictionary<string, string> ParseConfigs(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new Dictionary<string, string>();

            try {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            } catch {
                return new Dictionary<string, string>();
            }
        }
    }
}