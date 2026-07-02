using System;
using System.Collections.Generic;
using System.Data;
using Plus.Database.Interfaces;
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
                DataTable data = null;
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT `items`.base_item, COALESCE(`items_groups`.`group_id`, 0) AS `group_id` FROM `items` LEFT OUTER JOIN `items_groups` ON `items`.`id` = `items_groups`.`id` WHERE `items`.`room_id` = @rid;");
                    dbClient.AddParameter("rid", roomId);
                    data = dbClient.GetTable();
                }

                Dictionary<int, int> roomItems = new();
                if (data != null) {
                    foreach (DataRow dRow in data.Rows)
                    {
                        int itemId = Convert.ToInt32(dRow["base_item"]);
                        if (roomItems.ContainsKey(itemId))
                            roomItems[itemId]++;
                        else
                            roomItems.Add(itemId, 1);
                    }
                }

                foreach (var roomItem in roomItems)
                {
                    items += roomItem.Key + "*" + roomItem.Value + ";";
                }

                if (roomItems.Count > 0) {
                    items = items.Remove(items.Length - 1);
                }
            }

            string[] splitItems = items.Split(';');
            foreach (string split in splitItems)
            {
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