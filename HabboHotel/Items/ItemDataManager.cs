using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;

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

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                foreach (var row in db.furnitures.AsNoTracking().ToList())
                {
                    try
                    {
                        int id = (int) row.id;
                        int spriteId = row.sprite_id.GetValueOrDefault();
                        string itemName = row.item_name;
                        string publicName = row.public_name;
                        string type = row.type;
                        int width = row.width;
                        int length = row.length;
                        double height = row.stack_height;
                        bool allowStack = PlusEnvironment.EnumToBool(row.can_stack.ToString());
                        bool allowWalk = PlusEnvironment.EnumToBool(row.is_walkable.ToString());
                        bool allowSit = PlusEnvironment.EnumToBool(row.can_sit.ToString());
                        bool allowRecycle = PlusEnvironment.EnumToBool(row.allow_recycle);
                        bool allowTrade = PlusEnvironment.EnumToBool(row.allow_trade);
                        bool allowMarketplace = Convert.ToInt32(row.allow_marketplace_sell) == 1;
                        bool allowGift = Convert.ToInt32(row.allow_gift) == 1;
                        bool allowInventoryStack = PlusEnvironment.EnumToBool(row.allow_inventory_stack);
                        InteractionType interactionType = InteractionTypes.GetTypeFromString(row.interaction_type);
                        int behaviourData = row.behaviour_data;
                        int cycleCount = row.interaction_modes_count;
                        string vendingIds = row.vending_ids;
                        string heightAdjustable = row.height_adjustable;
                        int effectId = row.effect_id;
                        bool isRare = PlusEnvironment.EnumToBool(row.is_rare);
                        bool extraRot = PlusEnvironment.EnumToBool(row.extra_rot);

                        if (!Gifts.ContainsKey(spriteId))
                            Gifts.Add(spriteId, new ItemData(id, spriteId, itemName, publicName, type, width, length, height, allowStack, allowWalk, allowSit, allowRecycle, allowTrade, allowMarketplace, allowGift, allowInventoryStack, interactionType, behaviourData, cycleCount, vendingIds, heightAdjustable, effectId, isRare, extraRot));

                        if (!Items.ContainsKey(id))
                            Items.Add(id, new ItemData(id, spriteId, itemName, publicName, type, width, length, height, allowStack, allowWalk, allowSit, allowRecycle, allowTrade, allowMarketplace, allowGift, allowInventoryStack, interactionType, behaviourData, cycleCount, vendingIds, heightAdjustable, effectId, isRare, extraRot));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error loading furniture ID {row.id}");
                        Console.WriteLine(e);
                        Console.ReadKey();
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