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
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var pets = db.bots_pet_responses.Select(p => new { p.pet_id, p.responses }).ToList();

                foreach (var row in pets)
                {
                    _values.Add(row.pet_id, row.responses.Split(';'));
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