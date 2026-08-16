using System;
using System.Collections.Generic;
using Plus.HabboHotel.Items.Wired;
using System.Globalization;

namespace Plus.HabboHotel.Items
{
    public class ItemData
    {
        public int Id { get; set; }
        public int SpriteId { get; set; }
        public string ItemName { get; set; }
        public string PublicName { get; set; }
        public char Type { get; set; }
        public int Width { get; set; }
        public int Length { get; set; }
        public double Height { get; set; }
        public bool Stackable { get; set; }

        /// <summary>
        /// furniture.is_walkable, as a tri-state rather than a flag:
        ///   0 — solid, you neither cross it nor stand on it;
        ///   1 — walk over it freely;
        ///   2 — you may stand on it, but not cross it. Paths route around such a tile unless it
        ///       is the destination, which is how you build a platform people step onto rather
        ///       than stream across.
        /// </summary>
        public byte WalkableState { get; set; }
        public bool Walkable => WalkableState > 0;

        public bool IsSeat { get; set; }
        public bool AllowEcotronRecycle { get; set; }
        public bool AllowTrade { get; set; }
        public bool AllowMarketplaceSell { get; set; }
        public bool AllowGift { get; set; }
        public bool AllowInventoryStack { get; set; }
        public InteractionType InteractionType { get; set; }
        public int BehaviourData { get; set; }
        public int Modes { get; set; }
        public List<int> VendingIds { get; set; }
        public List<double> AdjustableHeights { get; set; }
        public bool HasAdjustableHeights => AdjustableHeights is { Count: > 0 };
        public int IsUsable { get; set; }

        public double HeightForState(int state)
        {
            if (!HasAdjustableHeights)
                return Height;

            int count = AdjustableHeights.Count;
            int index = ((state % count) + count) % count;

            return AdjustableHeights[index];
        }

        public int EffectId { get; set; }
        public WiredBoxType WiredType { get; set; }
        public bool IsRare { get; set; }
        public bool ExtraRot { get; set; }

        public ItemData(int id, int sprite, string name, string publicName, string type, int width, int length, double height, bool stackable, byte walkableState, bool isSeat,
            bool allowRecycle, bool allowTrade, bool allowMarketplaceSell, bool allowGift, bool allowInventoryStack, InteractionType interactionType, int behaviourData, int modes,
            string vendingIds, string adjustableHeights, int effectId, bool isRare, bool extraRot)
        {
            Id = id;
            SpriteId = sprite;
            ItemName = name;
            PublicName = publicName;
            Type = char.Parse(type);
            Width = width;
            Length = length;
            Height = height;
            Stackable = stackable;
            WalkableState = walkableState;
            IsSeat = isSeat;
            AllowEcotronRecycle = allowRecycle;
            AllowTrade = allowTrade;
            AllowMarketplaceSell = allowMarketplaceSell;
            AllowGift = allowGift;
            AllowInventoryStack = allowInventoryStack;
            InteractionType = interactionType;
            BehaviourData = behaviourData;
            Modes = modes;
            VendingIds = new List<int>();
            if (!string.IsNullOrEmpty(vendingIds)) {
                foreach (string vendingId in vendingIds.Split(',')) {
                    if (int.TryParse(vendingId, out int idValue))
                        VendingIds.Add(idValue);
                    else
                        Console.WriteLine("Error with Item " + ItemName + " - Vending Ids");
                }
            }

            AdjustableHeights = new List<double>();
            if (!string.IsNullOrEmpty(adjustableHeights)) {
                foreach (string h in adjustableHeights.Split(',')) {
                    if (double.TryParse(h, NumberStyles.Any, CultureInfo.InvariantCulture, out double heightValue))
                        AdjustableHeights.Add(heightValue);
                    else
                        Console.WriteLine("Error with Item " + ItemName + " - Adjustable Height: " + h);
                }
            }

            if (AdjustableHeights.Count < 2 || AdjustableHeights.TrueForAll(static h => h <= 0.0))
                AdjustableHeights.Clear();

            EffectId = effectId;

            int wiredId = 0;
            if (InteractionType == InteractionType.WiredCondition || InteractionType == InteractionType.WiredTrigger || InteractionType == InteractionType.WiredEffect)
                wiredId = BehaviourData;

            WiredType = WiredBoxTypeUtility.FromWiredId(wiredId);

            IsRare = isRare;
            ExtraRot = extraRot;
            IsUsable = InteractionType switch
            {
                InteractionType.Teleport or
                InteractionType.VendingMachine or
                InteractionType.WfFloorSwitch1 or
                InteractionType.WfFloorSwitch2 or
                InteractionType.Dice or
                InteractionType.RpVendor or
                InteractionType.TrashBin or
                InteractionType.Crafting or
                InteractionType.CraftingCorporation => 2,
                _ => Modes > 1 ? 0 : 1
            };
        }
    }
}