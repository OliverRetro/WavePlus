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
        public bool Walkable { get; set; }
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
        public int EffectId { get; set; }
        public WiredBoxType WiredType { get; set; }
        public bool IsRare { get; set; }
        public bool ExtraRot { get; set; }

        public ItemData(int id, int sprite, string name, string publicName, string type, int width, int length, double height, bool stackable, bool walkable, bool isSeat,
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
            Walkable = walkable;
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

            EffectId = effectId;

            int wiredId = 0;
            if (InteractionType == InteractionType.WiredCondition || InteractionType == InteractionType.WiredTrigger || InteractionType == InteractionType.WiredEffect)
                wiredId = BehaviourData;

            WiredType = WiredBoxTypeUtility.FromWiredId(wiredId);

            IsRare = isRare;
            ExtraRot = extraRot;
        }
    }
}