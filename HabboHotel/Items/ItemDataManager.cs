using System;
using System.Collections.Generic;
using System.Data;
using log4net;
using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Items
{
    public class ItemDataManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ItemDataManager));

        public Dictionary<int, ItemData> Items;
        public Dictionary<int, ItemData> Gifts; //<SpriteId, Item>

        public ItemDataManager()
        {
            Items = new Dictionary<int, ItemData>();
            Gifts = new Dictionary<int, ItemData>();
        }

        public void Init()
        {
            if (Items.Count > 0)
                Items.Clear();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `furniture`");
                DataTable itemData = dbClient.GetTable();

                if (itemData != null) {
                    foreach (DataRow row in itemData.Rows)
                    {
                        try
                        {
                            int id = Convert.ToInt32(row["id"]);
                            int spriteId = Convert.ToInt32(row["sprite_id"]);
                            string itemName = Convert.ToString(row["item_name"]);
                            string publicName = Convert.ToString(row["public_name"]);
                            string type = row["type"].ToString();
                            int width = Convert.ToInt32(row["width"]);
                            int length = Convert.ToInt32(row["length"]);
                            double height = Convert.ToDouble(row["stack_height"]);
                            bool allowStack = PlusEnvironment.EnumToBool(row["can_stack"].ToString());
                            bool allowWalk = PlusEnvironment.EnumToBool(row["is_walkable"].ToString());
                            bool allowSit = PlusEnvironment.EnumToBool(row["can_sit"].ToString());
                            bool allowRecycle = PlusEnvironment.EnumToBool(row["allow_recycle"].ToString());
                            bool allowTrade = PlusEnvironment.EnumToBool(row["allow_trade"].ToString());
                            bool allowMarketplace = Convert.ToInt32(row["allow_marketplace_sell"]) == 1;
                            bool allowGift = Convert.ToInt32(row["allow_gift"]) == 1;
                            bool allowInventoryStack = PlusEnvironment.EnumToBool(row["allow_inventory_stack"].ToString());
                            InteractionType interactionType = InteractionTypes.GetTypeFromString(Convert.ToString(row["interaction_type"]));
                            int behaviourData = Convert.ToInt32(row["behaviour_data"]);
                            int cycleCount = Convert.ToInt32(row["interaction_modes_count"]);
                            string vendingIds = Convert.ToString(row["vending_ids"]);
                            string heightAdjustable = Convert.ToString(row["height_adjustable"]);
                            int effectId = Convert.ToInt32(row["effect_id"]);
                            bool isRare = PlusEnvironment.EnumToBool(row["is_rare"].ToString());
                            bool extraRot = PlusEnvironment.EnumToBool(row["extra_rot"].ToString());

                            if (!Gifts.ContainsKey(spriteId))
                                Gifts.Add(spriteId, new ItemData(id, spriteId, itemName, publicName, type, width, length, height, allowStack, allowWalk, allowSit, allowRecycle, allowTrade, allowMarketplace, allowGift, allowInventoryStack, interactionType, behaviourData, cycleCount, vendingIds, heightAdjustable, effectId, isRare, extraRot));

                            if (!Items.ContainsKey(id))
                                Items.Add(id, new ItemData(id, spriteId, itemName, publicName, type, width, length, height, allowStack, allowWalk, allowSit, allowRecycle, allowTrade, allowMarketplace, allowGift, allowInventoryStack, interactionType, behaviourData, cycleCount, vendingIds, heightAdjustable, effectId, isRare, extraRot));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Error loading furniture ID {row["id"]}");
                            foreach (DataColumn col in row.Table.Columns)
                                Console.WriteLine($"{col.ColumnName}: {row[col]}");

                            Console.WriteLine(e);
                            Console.ReadKey();
                        }
                    }
                }
            }

            Log.Info("Item Manager -> LOADED");
        }

        public bool GetItem(int id, out ItemData item)
        {
            return Items.TryGetValue(id, out item);
        }

        public ItemData GetItemByName(string name)
        {
            foreach (var entry in Items)
            {
                ItemData item = entry.Value;
                if (item.ItemName == name)
                    return item;
            }

            return null;
        }

        public bool GetGift(int spriteId, out ItemData item)
        {
            return Gifts.TryGetValue(spriteId, out item);
        }
    }
}