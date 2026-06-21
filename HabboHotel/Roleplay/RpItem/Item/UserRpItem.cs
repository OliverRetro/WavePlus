namespace Plus.HabboHotel.Roleplay.RpItem.Item
{
    public class UserRpItem
    {
        public int Id { get; set; }
        public int UserId { get; }
        public int ItemId { get; }

        public int Uses { get; set; }

        public RpItemData ItemData { get; }

        public UserRpItem(int id, int userId, int itemId, int uses, RpItemData itemData)
        {
            Id = id;
            UserId = userId;
            ItemId = itemId;
            Uses = uses;
            ItemData = itemData;
        }
    }
}
