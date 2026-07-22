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

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var rows = db.CatalogPetRaces.Select(r => new { r.Raceid, r.Color1, r.Color2, r.Has1color, r.Has2color }).ToList();
                foreach (var row in rows) {
                    PetRace race = new(row.Raceid ?? 0, row.Color1 ?? 0, row.Color2 ?? 0, row.Has1color == "1", row.Has2color == "1");
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