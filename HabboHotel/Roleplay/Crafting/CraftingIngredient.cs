namespace Plus.HabboHotel.Roleplay.Crafting
{
    public sealed class CraftingIngredient
    {
        public string ItemType { get; }

        public int ItemId { get; }

        public int Amount { get; }

        public CraftingIngredient(string itemType, int itemId, int amount)
        {
            ItemType = itemType;
            ItemId = itemId;
            Amount = amount < 1 ? 1 : amount;
        }

        public string Key => $"{ItemType}:{ItemId}";
    }
}