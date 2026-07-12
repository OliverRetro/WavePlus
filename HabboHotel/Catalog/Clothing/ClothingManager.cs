using System.Collections.Generic;
using System.Linq;
using Plus.Database.EF;

namespace Plus.HabboHotel.Catalog.Clothing
{
    public class ClothingManager
    {
        private readonly Dictionary<int, ClothingItem> _clothing;

        public ClothingManager()
        {
            _clothing = new Dictionary<int, ClothingItem>();
        }

        public void Init()
        {
            if (_clothing.Count > 0)
                _clothing.Clear();

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var rows = db.catalog_clothings.Select(c => new { c.id, c.clothing_name, c.clothing_parts }).ToList();
                foreach (var row in rows)
                {
                    _clothing.Add(row.id, new ClothingItem(row.id, row.clothing_name, row.clothing_parts));
                }
            }
        }

        public bool TryGetClothing(int itemId, out ClothingItem clothing)
        {
            return _clothing.TryGetValue(itemId, out clothing);
        }

        public ICollection<ClothingItem> GetClothingAllParts => _clothing.Values;
    }
}