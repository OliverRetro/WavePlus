using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Crafting;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Crafting;

namespace Plus.Communication.Packets.Incoming.Crafting
{
    internal class GetCraftingRecipeEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            string recipeName = packet.PopString();

            CraftingRecipe recipe = PlusEnvironment.GetCraftingManager().GetByName(recipeName);
            if (recipe == null)
                return;

            List<(int Amount, string Key, string DisplayName, string IconUrl)> ingredients = new();
            foreach (CraftingIngredient ingredient in recipe.Ingredients) {
                if (CraftingManager.Describe(ingredient.ItemType, ingredient.ItemId, out string name, out string icon, out _))
                    ingredients.Add((ingredient.Amount, ingredient.Key, name, icon));
            }

            session.SendPacket(new CraftingRecipeComposer(ingredients));
        }
    }
}