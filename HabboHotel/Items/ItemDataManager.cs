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

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                foreach (var row in db.Furnitures.AsNoTracking().ToList()) {
                    try {
                        int id = (int)row.Id;
                        int spriteId = row.SpriteId.GetValueOrDefault();
                        string itemName = row.ItemName;
                        string publicName = row.PublicName;
                        string type = row.Type;
                        int width = row.Width;
                        int length = row.Length;
                        double height = row.StackHeight;
                        bool allowStack = PlusEnvironment.EnumToBool(row.CanStack.ToString());
                        // is_walkable is a tinyint, not a flag: 2 means "stand on it, but paths go
                        // around it". Anything unrecognised falls back to solid.
                        byte walkableState = (byte)Math.Clamp((int)row.IsWalkable.GetValueOrDefault(), 0, 2);
                        bool allowSit = PlusEnvironment.EnumToBool(row.CanSit.ToString());
                        bool allowRecycle = PlusEnvironment.EnumToBool(row.AllowRecycle);
                        bool allowTrade = PlusEnvironment.EnumToBool(row.AllowTrade);
                        bool allowMarketplace = Convert.ToInt32(row.AllowMarketplaceSell) == 1;
                        bool allowGift = Convert.ToInt32(row.AllowGift) == 1;
                        bool allowInventoryStack = PlusEnvironment.EnumToBool(row.AllowInventoryStack);
                        InteractionType interactionType = InteractionTypes.GetTypeFromString(row.InteractionType);
                        int behaviourData = row.BehaviourData;
                        int cycleCount = row.InteractionModesCount;
                        string vendingIds = row.VendingIds;
                        string heightAdjustable = row.HeightAdjustable;
                        int effectId = row.EffectId;
                        bool isRare = PlusEnvironment.EnumToBool(row.IsRare);
                        bool extraRot = PlusEnvironment.EnumToBool(row.ExtraRot);

                        if (!Gifts.ContainsKey(spriteId))
                            Gifts.Add(spriteId, new ItemData(id, spriteId, itemName, publicName, type, width, length, height, allowStack, walkableState, allowSit, allowRecycle, allowTrade, allowMarketplace, allowGift, allowInventoryStack, interactionType, behaviourData, cycleCount, vendingIds, heightAdjustable, effectId, isRare, extraRot));

                        if (!Items.ContainsKey(id))
                            Items.Add(id, new ItemData(id, spriteId, itemName, publicName, type, width, length, height, allowStack, walkableState, allowSit, allowRecycle, allowTrade, allowMarketplace, allowGift, allowInventoryStack, interactionType, behaviourData, cycleCount, vendingIds, heightAdjustable, effectId, isRare, extraRot));
                    } catch (Exception e) {
                        Console.WriteLine($"Error loading furniture ID {row.Id}");
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
            foreach (var entry in Items) {
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