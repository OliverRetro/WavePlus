using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Crafting;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Crafting;
using Plus.HabboHotel.Roleplay.RpItem;
using Plus.HabboHotel.Roleplay.Utilities;

namespace Plus.Communication.Packets.Incoming.Crafting
{
    internal class GetCraftableProductsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int objectId = packet.PopInt();

            ICraftingSource source = CraftingContext.Resolve(session, objectId);
            if (source == null)
                return;

            CraftingManager manager = PlusEnvironment.GetCraftingManager();

            List<(string Name, string Key, string DisplayName, string IconUrl)> recipes = new();
            List<(string Key, string DisplayName, string IconUrl, int Count)> pool = new();
            HashSet<(string, int)> seen = new();

            foreach (CraftingRecipe recipe in manager.AllRecipes) {
                if (!source.IsUnlocked(recipe.Id))
                    continue;

                foreach (CraftingIngredient ingredient in recipe.Ingredients) {
                    if (!seen.Add((ingredient.ItemType, ingredient.ItemId)))
                        continue;

                    if (CraftingManager.Describe(ingredient.ItemType, ingredient.ItemId, out string iName, out string iIcon, out _))
                        pool.Add((ingredient.Key, iName, iIcon, source.CountOwned(ingredient.ItemType, ingredient.ItemId)));
                }

                if (source.IsVisible(recipe) &&
                    CraftingManager.Describe(recipe.RewardType, recipe.RewardId, out string rName, out string rIcon, out _))
                    recipes.Add((recipe.Name, $"{recipe.RewardType}:{recipe.RewardId}", rName, rIcon));
            }

            int craftSeconds = RpInteractionTimer.GetSeconds(session.GetHabbo());

            bool inventoryFull = source is PersonalCraftingSource &&
                RpInventory.UsedSlots(session.GetHabbo(), 10) >= 10;

            session.SendPacket(new CraftableProductsComposer(recipes, pool, craftSeconds, inventoryFull));
        }
    }
}