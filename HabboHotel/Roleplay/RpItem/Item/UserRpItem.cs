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

        public RpItemData ItemData { get; }

        public UserRpItem(int id, int userId, int itemId, int uses, RpItemData itemData, int slot = -1)
        {
            Id = id;
            UserId = userId;
            ItemId = itemId;
            Uses = uses;
            ItemData = itemData;
            Slot = slot;
        }
    }
}
