using System.Collections.Generic;
using System.Data;
using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Rooms.Chat.Pets.Locale
{
    public class PetLocale
    {
        private Dictionary<string, string[]> _values;

        public PetLocale()
        {
            _values = new Dictionary<string, string[]>();

            Init();
        }

        public void Init()
        {
            _values = new Dictionary<string, string[]>();
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `bots_pet_responses`");
                DataTable pets = dbClient.GetTable();

                if (pets != null) {
                    foreach (DataRow row in pets.Rows)
                    {
                        _values.Add(row[0].ToString(), row[1].ToString().Split(';'));
                    }
                }
            }
        }

        public string[] GetValue(string key)
        {
            if (_values.TryGetValue(key, out string[] value))
                return value;
            return new[] {"Unknown pet speach:" + key};
        }
    }
}