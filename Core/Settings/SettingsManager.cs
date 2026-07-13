using System.Collections.Generic;
using System.Linq;
using log4net;
using Plus.Database.EF;

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

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var rows = db.ServerSettings.Select(s => new { s.Key, s.Value }).ToList();
                foreach (var row in rows)
                {
                    _settings.Add(row.Key.ToLower(), row.Value.ToLower());
                }
            }

            Log.Info("Loaded " + _settings.Count + " server settings.");
        }

        public string TryGetValue(string value)
        {
            return _settings.TryGetValue(value, out string value1) ? value1 : "0";
        }
    }
}
