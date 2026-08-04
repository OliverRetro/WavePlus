using System.Collections.Generic;
using System.Linq;
using log4net;
using Plus.Database.EF;
using Plus.Database.EF.Entities;

namespace Plus.Core.Settings
{
    public class SettingsManager
    {
        private readonly Dictionary<string, string> _settings;

        private static readonly ILog Log = LogManager.GetLogger(typeof(SettingsManager));

        public SettingsManager()
        {
            _settings = [];
        }

        public void Init()
        {
            if (_settings.Count > 0)
                _settings.Clear();

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var rows = db.ServerSettings.Select(s => new { s.Key, s.Value }).ToList();
                foreach (var row in rows) {
                    _settings.Add(row.Key.ToLower(), row.Value.ToLower());
                }
            }

            Log.Info("Loaded " + _settings.Count + " server settings.");
        }

        public string TryGetValue(string value)
        {
            return _settings.TryGetValue(value, out string value1) ? value1 : "0";
        }

        public bool ContainsKey(string key) => _settings.ContainsKey(key);

        public void SetValue(string key, string value, string description = "")
        {
            key = key.ToLower();

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var existing = db.ServerSettings.FirstOrDefault(s => s.Key == key);
                if (existing != null) {
                    existing.Value = value;
                } else {
                    db.ServerSettings.Add(new ServerSettingEntity
                    {
                        Key = key,
                        Value = value,
                        Description = description ?? ""
                    });
                }
                db.SaveChanges();
            }

            _settings[key] = value.ToLower();
        }
    }
}