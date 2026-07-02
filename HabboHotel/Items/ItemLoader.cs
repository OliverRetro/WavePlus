using System;
using System.Collections.Generic;
using System.Data;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Items
{
    public static class ItemLoader
    {
        public static List<Item> GetItemsForRoom(int roomId, Room room)
        {
            List<Item> items = new();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `items`.*, COALESCE(`items_groups`.`group_id`, 0) AS `group_id` FROM `items` LEFT OUTER JOIN `items_groups` ON `items`.`id` = `items_groups`.`id` WHERE `items`.`room_id` = @rid;");
                dbClient.AddParameter("rid", roomId);
                DataTable data = dbClient.GetTable();

                if (data != null) {
                    foreach (DataRow row in data.Rows)
                    {
                        if (PlusEnvironment.GetGame().GetItemManager().GetItem(Convert.ToInt32(row["base_item"]), out ItemData Data)) {
                            items.Add(new Item(Convert.ToInt32(row["id"]), Convert.ToInt32(row["room_id"]), Convert.ToInt32(row["base_item"]), Convert.ToString(row["extra_data"]),
                                Convert.ToInt32(row["x"]), Convert.ToInt32(row["y"]), Convert.ToDouble(row["z"]), Convert.ToInt32(row["rot"]), Convert.ToInt32(row["user_id"]),
                                Convert.ToInt32(row["group_id"]), Convert.ToInt32(row["limited_number"]), Convert.ToInt32(row["limited_stack"]), Convert.ToString(row["wall_pos"]), room));
                        }
                    }
                }
            }

            return items;
        }

        public static List<Item> GetItemsForUser(int userId)
        {
            List<Item> I = new();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `items`.*, COALESCE(`items_groups`.`group_id`, 0) AS `group_id` FROM `items` LEFT OUTER JOIN `items_groups` ON `items`.`id` = `items_groups`.`id` WHERE `items`.`room_id` = 0 AND `items`.`user_id` = @uid;");
                dbClient.AddParameter("uid", userId);
                DataTable items = dbClient.GetTable();

                if (items != null) {
                    foreach (DataRow row in items.Rows)
                    {
                        if (PlusEnvironment.GetGame().GetItemManager().GetItem(Convert.ToInt32(row["base_item"]), out ItemData data)) {
                            I.Add(new Item(Convert.ToInt32(row["id"]), Convert.ToInt32(row["room_id"]), Convert.ToInt32(row["base_item"]), Convert.ToString(row["extra_data"]),
                                Convert.ToInt32(row["x"]), Convert.ToInt32(row["y"]), Convert.ToDouble(row["z"]), Convert.ToInt32(row["rot"]), Convert.ToInt32(row["user_id"]),
                                Convert.ToInt32(row["group_id"]), Convert.ToInt32(row["limited_number"]), Convert.ToInt32(row["limited_stack"]), Convert.ToString(row["wall_pos"])));
                        }
                    }
                }
            }

            return I;
        }
    }
}