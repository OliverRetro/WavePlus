using System.Collections.Generic;
using System.Linq;
using log4net;
using Plus.Database.EF;

namespace Plus.Core.Language
{
    public class LanguageManager
    {
        private readonly Dictionary<string, string> _values;

        private static readonly ILog Log = LogManager.GetLogger(typeof(LanguageManager));

        public LanguageManager()
        {
            _values = new Dictionary<string, string>();
        }

        public void Init()
        {
            if (_values.Count > 0)
                _values.Clear();

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var rows = db.server_locales.Select(l => new { l.key, l.value }).ToList();
                foreach (var row in rows)
                {
                    _values.Add(row.key, row.value);
                }
            }

            Log.Info("Loaded " + _values.Count + " language locales.");
        }

        public string TryGetValue(string value)
        {
            return _values.ContainsKey(value) ? _values[value] : "No language locale found for [" + value + "]";
        }
    }
}
