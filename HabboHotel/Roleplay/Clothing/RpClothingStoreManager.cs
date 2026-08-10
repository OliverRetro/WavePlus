using System.Collections.Generic;
using System.Linq;
using log4net;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;

namespace Plus.HabboHotel.Roleplay.Clothing
{
    public sealed class RpClothingStoreManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RpClothingStoreManager));

        private readonly Dictionary<int, RpClothingCategory> _categories = [];
        private readonly Dictionary<int, RpClothingSet> _sets = [];

        // room id -> ordered categories in that room
        private readonly Dictionary<int, List<RpClothingCategory>> _byRoom = [];

        public void Init()
        {
            _categories.Clear();
            _sets.Clear();
            _byRoom.Clear();

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                foreach (var row in db.RpClothingCategories.AsNoTracking().OrderBy(c => c.Id).ToList()) {
                    RpClothingCategory category = new(row.Id, row.TabName, row.RoomId);
                    _categories[category.Id] = category;

                    if (!_byRoom.TryGetValue(category.RoomId, out List<RpClothingCategory> list)) {
                        list = new List<RpClothingCategory>();
                        _byRoom[category.RoomId] = list;
                    }

                    list.Add(category);
                }

                foreach (var row in db.RpClothingSets.AsNoTracking().OrderBy(s => s.Id).ToList()) {
                    RpClothingSet set = new(
                        row.Id,
                        row.TabId ?? 0,
                        row.SetType,
                        row.PartTypes,
                        row.Color,
                        row.Name,
                        row.Stock,
                        row.BasePrice,
                        row.DiscountPrice,
                        row.Visible);

                    _sets[set.Id] = set;

                    if (set.Visible && _categories.TryGetValue(set.TabId, out RpClothingCategory category))
                        category.Sets.Add(set);
                }
            }

            Log.Info($"[RP] Loaded {_categories.Count} clothing tabs and {_sets.Count} clothing sets.");
        }

        public IReadOnlyList<RpClothingCategory> GetCategoriesForRoom(int roomId)
        {
            return _byRoom.TryGetValue(roomId, out List<RpClothingCategory> list)
                ? list
                : (IReadOnlyList<RpClothingCategory>)[];
        }

        public bool TryGetSet(int setId, out RpClothingSet set) => _sets.TryGetValue(setId, out set);

        public bool TryGetCategory(int categoryId, out RpClothingCategory category) => _categories.TryGetValue(categoryId, out category);

        public bool IsSetAvailableInRoom(RpClothingSet set, int roomId)
        {
            return set != null
                && set.Visible
                && _categories.TryGetValue(set.TabId, out RpClothingCategory category)
                && category.RoomId == roomId;
        }

        public void ConsumeStock(RpClothingSet set)
        {
            if (set == null || set.Stock <= 0)
                return;

            set.Stock--;

            int id = set.Id;
            int newStock = set.Stock;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.RpClothingSets.Where(s => s.Id == id).ExecuteUpdate(s => s.SetProperty(x => x.Stock, newStock));
            }
        }
    }
}