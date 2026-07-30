using System.Collections.Generic;

namespace Plus.HabboHotel.Roleplay.Crafting
{
    public sealed class CraftingRecipe
    {
        public int Id { get; }
        public string Name { get; }
        public string RewardType { get; }
        public int RewardId { get; }
        public int RewardAmount { get; }
        public bool Secret { get; }

        public List<CraftingIngredient> Ingredients { get; } = new();

        public CraftingRecipe(int id, string name, string rewardType, int rewardId, int rewardAmount, bool secret)
        {
            Id = id;
            Name = name;
            RewardType = string.IsNullOrWhiteSpace(rewardType) ? "rp_item" : rewardType;
            RewardId = rewardId;
            RewardAmount = rewardAmount < 1 ? 1 : rewardAmount;
            Secret = secret;
        }

        public Dictionary<(string Type, int Id), int> AsMultiset()
        {
            Dictionary<(string, int), int> map = [];
            foreach (CraftingIngredient ingredient in Ingredients) {
                map.TryGetValue((ingredient.ItemType, ingredient.ItemId), out int have);
                map[(ingredient.ItemType, ingredient.ItemId)] = have + ingredient.Amount;
            }
            return map;
        }
    }
}