using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;

namespace Plus.HabboHotel.Roleplay.RpItem.Item
{
    /// <summary>
    /// Loads and caches every RP item definition (`rp_items`).
    /// </summary>
    public sealed class RpItemManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RpItemManager));

        private readonly Dictionary<int, RpItemData> _items = new();

        public void Init()
        {
            _items.Clear();

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var rows = db.rp_items.AsNoTracking().ToList();

                foreach (var row in rows)
                {
                    RpItemData item = new(
                        row.id,
                        row.item_name,
                        Convert.ToInt32(row.handitem_data),
                        Convert.ToInt32(row.effect_data),
                        row.base_price,
                        row.image_url,
                        row.attribute_effects,
                        row.stack_limit,
                        row.item_type ?? "consumable"
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
