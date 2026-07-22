using System.Collections.Generic;
using System.Linq;
using Plus.Database.EF;

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
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var pets = db.BotsPetResponses.Select(p => new { p.PetId, p.Responses }).ToList();

                foreach (var row in pets) {
                    _values.Add(row.PetId, row.Responses.Split(';'));
                }
            }
        }

        public string[] GetValue(string key)
        {
            if (_values.TryGetValue(key, out string[] value))
                return value;
            return new[] { "Unknown pet speach:" + key };
        }
    }
}