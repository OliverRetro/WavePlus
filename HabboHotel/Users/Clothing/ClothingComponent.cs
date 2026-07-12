using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Plus.Database.EF;
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

            int uid = habbo.Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                foreach (var row in db.user_clothings.Where(c => c.user_id == uid).Select(c => new { c.id, c.part_id, c.part }).ToList())
                    _allClothing.TryAdd(Convert.ToInt32(row.part_id), new ClothingParts(row.id, Convert.ToInt32(row.part_id), row.part));
            }

            _habbo = habbo;
            return true;
        }

        public void AddClothing(string clothingName, List<int> partIds)
        {
            foreach (int partId in partIds.ToList())
            {
                if (!_allClothing.ContainsKey(partId)) {
                    int newId;
                    using (WavePlusContext db = PlusEnvironment.GetDbContext())
                    {
                        var entity = new Database.EF.Entities.user_clothing { user_id = _habbo.Id, part_id = partId.ToString(), part = clothingName };
                        db.user_clothings.Add(entity);
                        db.SaveChanges();
                        newId = entity.id;
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