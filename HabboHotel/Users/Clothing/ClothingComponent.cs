using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Users.Clothing.Parts;

namespace Plus.HabboHotel.Users.Clothing
{
    public sealed class ClothingComponent
    {
        private Habbo _habbo;

        /// <summary>
        /// Effects stored by ID > Effect.
        /// </summary>
        private readonly ConcurrentDictionary<int, ClothingParts> _allClothing = new();

        /// <summary>
        /// Initializes the EffectsComponent.
        /// </summary>
        /// <param name="habbo"></param>
        public bool Init(Habbo habbo)
        {
            if (_allClothing.Count > 0)
                return false;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`part_id`,`part` FROM `user_clothing` WHERE `user_id` = @id;");
                dbClient.AddParameter("id", habbo.Id);
                DataTable getClothing = dbClient.GetTable();

                if (getClothing != null) {
                    foreach (DataRow row in getClothing.Rows)
                    {
                        if (_allClothing.TryAdd(Convert.ToInt32(row["part_id"]), new ClothingParts(Convert.ToInt32(row["id"]), Convert.ToInt32(row["part_id"]), Convert.ToString(row["part"])))) {
                            //umm?
                        }
                    }
                }
            }

            _habbo = habbo;
            return true;
        }

        public void AddClothing(string clothingName, List<int> partIds)
        {
            foreach (int partId in partIds.ToList())
            {
                if (!_allClothing.ContainsKey(partId)) {
                    int newId = 0;
                    using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("INSERT INTO `user_clothing` (`user_id`,`part_id`,`part`) VALUES (@UserId, @PartId, @Part)");
                        dbClient.AddParameter("UserId", _habbo.Id);
                        dbClient.AddParameter("PartId", partId);
                        dbClient.AddParameter("Part", clothingName);
                        newId = Convert.ToInt32(dbClient.InsertQuery());
                    }

                    _allClothing.TryAdd(partId, new ClothingParts(newId, partId, clothingName));
                }
            }
        }

        public bool TryGet(int partId, out ClothingParts clothingPart)
        {
            return _allClothing.TryGetValue(partId, out clothingPart);
        }

        public ICollection<ClothingParts> GetClothingParts => _allClothing.Values;

        /// <summary>
        /// Disposes the ClothingComponent.
        /// </summary>
        public void Dispose()
        {
            _allClothing.Clear();
        }
    }
}