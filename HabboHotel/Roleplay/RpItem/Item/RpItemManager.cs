using System;
using System.Collections.Generic;
using System.Data;
using log4net;
using Plus.Database.Interfaces;

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

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `rp_items`");
                DataTable table = dbClient.GetTable();

                if (table != null)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        RpItemData item = new(
                            Convert.ToInt32(row["id"]),
                            Convert.ToString(row["item_name"]),
                            Convert.ToInt32(row["handitem_data"]),
                            Convert.ToInt32(row["effect_data"]),
                            Convert.ToInt32(row["base_price"]),
                            Convert.ToString(row["image_url"]),
                            Convert.ToString(row["attribute_effects"])
                        );

                        _items[item.Id] = item;
                    }
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
