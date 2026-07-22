using System.Collections.Generic;
using System.Linq;
using Plus.Database.EF;
using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Catalog
{
    public class CatalogDeal
    {
        public int Id { get; set; }
        public List<CatalogItem> ItemDataList { get; }
        public string DisplayName { get; set; }
        public int RoomId { get; set; }

        public CatalogDeal(int id, string items, string displayName, int roomId, ItemDataManager itemDataManager)
        {
            Id = id;
            DisplayName = displayName;
            RoomId = roomId;
            ItemDataList = new List<CatalogItem>();

            if (roomId != 0) {
                uint rid = (uint)roomId;
                // NOTE: original query LEFT JOINed items_groups but only used items.base_item; group_id was never read.
                List<uint> baseItems;
                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    baseItems = db.Items.Where(i => i.RoomId == rid).Select(i => i.BaseItem).ToList();
                }

                Dictionary<int, int> roomItems = new();
                foreach (uint baseItem in baseItems) {
                    int itemId = (int)baseItem;
                    if (roomItems.ContainsKey(itemId))
                        roomItems[itemId]++;
                    else
                        roomItems.Add(itemId, 1);
                }

                foreach (var roomItem in roomItems) {
                    items += roomItem.Key + "*" + roomItem.Value + ";";
                }

                if (roomItems.Count > 0) {
                    items = items.Remove(items.Length - 1);
                }
            }

            string[] splitItems = items.Split(';');
            foreach (string split in splitItems) {
                string[] item = split.Split('*');
                if (!int.TryParse(item[0], out int itemId) || !int.TryParse(item[1], out int amount))
                    continue;

                if (!itemDataManager.GetItem(itemId, out ItemData data))
                    continue;

                ItemDataList.Add(new CatalogItem(0, itemId, data, string.Empty, 0, 0, 0, 0, amount, 0, 0, false, "", "", 0));
            }
        }
    }
}