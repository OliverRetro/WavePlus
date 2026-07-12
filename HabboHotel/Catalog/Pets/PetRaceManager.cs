using System.Collections.Generic;
using System.Linq;
using Plus.Database.EF;

namespace Plus.HabboHotel.Catalog.Pets
{
    public class PetRaceManager
    {
        private readonly List<PetRace> _races = new();

        public void Init()
        {
            if (_races.Count > 0)
                _races.Clear();

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var rows = db.catalog_pet_races.Select(r => new { r.raceid, r.color1, r.color2, r.has1color, r.has2color }).ToList();
                foreach (var row in rows)
                {
                    PetRace race = new(row.raceid ?? 0, row.color1 ?? 0, row.color2 ?? 0, row.has1color == "1", row.has2color == "1");
                    if (!_races.Contains(race))
                        _races.Add(race);
                }
            }
        }

        public List<PetRace> GetRacesForRaceId(int raceId)
        {
            return _races.Where(race => race.RaceId == raceId).ToList();
        }
    }
}