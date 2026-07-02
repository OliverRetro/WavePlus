using System.Collections.Generic;
using System.Data;
using log4net;
using Plus.Database.Interfaces;

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

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `server_locale`");
                DataTable table = dbClient.GetTable();

                if (table != null) {
                    foreach (DataRow row in table.Rows)
                    {
                        _values.Add(row["key"].ToString(), row["value"].ToString());
                    }
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