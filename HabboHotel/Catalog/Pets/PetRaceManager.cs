using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Catalog.Pets
{
    public class PetRaceManager
    {
        private readonly List<PetRace> _races = new();

        public void Init()
        {
            if (_races.Count > 0)
                _races.Clear();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `catalog_pet_races`");
                DataTable data = dbClient.GetTable();

                if (data != null) {
                    foreach (DataRow row in data.Rows)
                    {
                        PetRace race = new(Convert.ToInt32(row["raceid"]), Convert.ToInt32(row["color1"]), Convert.ToInt32(row["color2"]), (Convert.ToString(row["has1color"]) == "1"), (Convert.ToString(row["has2color"]) == "1"));
                        if (!_races.Contains(race))
                            _races.Add(race);
                    }
                }
            }
        }

        public List<PetRace> GetRacesForRaceId(int raceId)
        {
            return _races.Where(race => race.RaceId == raceId).ToList();
        }
    }
}