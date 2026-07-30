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

        private readonly ConcurrentDictionary<int, ClothingParts> _allClothing = new();

        public bool Init(Habbo habbo)
        {
            if (_allClothing.Count > 0)
                return false;

            int uid = habbo.Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                foreach (var row in db.UserClothings.Where(c => c.UserId == uid).Select(c => new { c.Id, c.PartId, c.Part }).ToList())
                    _allClothing.TryAdd(Convert.ToInt32(row.PartId), new ClothingParts(row.Id, Convert.ToInt32(row.PartId), row.Part));
            }

            _habbo = habbo;
            return true;
        }

        public void AddClothing(string clothingName, List<int> partIds)
        {
            foreach (int partId in partIds.ToList()) {
                if (!_allClothing.ContainsKey(partId)) {
                    int newId;
                    using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                        var entity = new Database.EF.Entities.UserClothingEntity { UserId = _habbo.Id, PartId = partId.ToString(), Part = clothingName };
                        db.UserClothings.Add(entity);
                        db.SaveChanges();
                        newId = entity.Id;
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

        public void Dispose()
        {
            _allClothing.Clear();
        }
    }
}