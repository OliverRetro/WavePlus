using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.Roleplay.Crafting;

namespace Plus.HabboHotel.Users
{
    public sealed class UserCraftingRecipes
    {
        private readonly int _userId;

        // recipeId -> revealed
        private readonly Dictionary<int, bool> _recipes;

        public UserCraftingRecipes(int userId, IEnumerable<(int RecipeId, bool Revealed)> loaded)
        {
            _userId = userId;
            _recipes = loaded.ToDictionary(x => x.RecipeId, x => x.Revealed);
        }

        public bool IsUnlocked(int recipeId) => _recipes.ContainsKey(recipeId);

        public bool IsRevealed(int recipeId) => _recipes.TryGetValue(recipeId, out bool revealed) && revealed;

        public bool IsVisible(CraftingRecipe recipe) =>
            recipe != null && IsUnlocked(recipe.Id) && (!recipe.Secret || IsRevealed(recipe.Id));

        public bool Unlock(int recipeId)
        {
            if (_recipes.ContainsKey(recipeId))
                return false;

            _recipes[recipeId] = false;

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            if (!db.UserCraftingRecipes.Any(u => u.UserId == _userId && u.RecipeId == recipeId)) {
                db.UserCraftingRecipes.Add(new UserCraftingRecipeEntity { UserId = _userId, RecipeId = recipeId, Revealed = false });
                db.SaveChanges();
            }

            return true;
        }

        public void Reveal(int recipeId)
        {
            if (!_recipes.TryGetValue(recipeId, out bool revealed) || revealed)
                return;

            _recipes[recipeId] = true;

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            db.UserCraftingRecipes
                .Where(u => u.UserId == _userId && u.RecipeId == recipeId)
                .ExecuteUpdate(s => s.SetProperty(u => u.Revealed, true));
        }
    }
}