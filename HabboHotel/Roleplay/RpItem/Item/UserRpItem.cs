namespace Plus.HabboHotel.Roleplay.RpItem.Item
{
    public class UserRpItem
    {
        public int Id { get; set; }
        public int UserId { get; }
        public int ItemId { get; }

        public int Uses { get; set; }

        // Inventory grid position (0-based). -1 = unassigned (placed into the first free slot).
        public int Slot { get; set; } = -1;

        // Per-copy durability used by shields. -1 = full / not yet initialised (use ItemData.MaxShield).
        public int DurabilityLeft { get; set; } = -1;

        // True when this item is equipped in the secondary slot (shields).
        public bool Equipped { get; set; }

        public RpItemData ItemData { get; }

        public UserRpItem(int id, int userId, int itemId, int uses, RpItemData itemData, int slot = -1, int durabilityLeft = -1, bool equipped = false)
        {
            Id = id;
            UserId = userId;
            ItemId = itemId;
            Uses = uses;
            ItemData = itemData;
            Slot = slot;
            DurabilityLeft = durabilityLeft;
            Equipped = equipped;
        }

        public int EffectiveDurability => DurabilityLeft < 0 ? ItemData?.MaxDurability ?? 0 : DurabilityLeft;
    }
}