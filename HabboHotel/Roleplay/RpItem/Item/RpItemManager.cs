using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;

namespace Plus.HabboHotel.Roleplay.RpItem.Item
{
    public sealed class RpItemManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RpItemManager));

        private readonly Dictionary<int, RpItemData> _items = new();

        public void Init()
        {
            _items.Clear();

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var rows = db.RpItems.AsNoTracking().ToList();

                foreach (var row in rows) {
                    RpItemData item = new(
                        row.Id,
                        row.ItemName,
                        Convert.ToInt32(row.HanditemData),
                        Convert.ToInt32(row.EffectData),
                        row.BasePrice,
                        row.ImageUrl,
                        row.AttributeEffects,
                        row.StackLimit,
                        row.ItemType ?? "consumable",
                        row.Rarity,
                        row.Bubble ?? string.Empty,
                        row.Clothing ?? string.Empty
                    );

                    _items[item.Id] = item;
                }
            }

            Log.Info($"[RP] Loaded {_items.Count} RP Items.");
        }

        public ICollection<RpItemData> GetItems() => _items.Values;

        public bool TryGetItem(int itemId, out RpItemData item) => _items.TryGetValue(itemId, out item);

        public RpItemData GetItemById(int itemId)
        {
            _items.TryGetValue(itemId, out RpItemData item);
            return item;
        }
    }
}