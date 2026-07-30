using System.Linq;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay
{
    internal class UnlockUserRecipeCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_unlock_user_recipe";

        public string Parameters => "%username% %recipeId%";

        public string Description => "Unlock a crafting recipe for a user.";

        public bool UsableWhileDead => true;

        public bool UsableWhileCuffed => true;

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 3 || !int.TryParse(@params[2], out int recipeId)) {
                session.SendWhisper("Usage: :unlockuserrecipe <username> <recipeId>", 1);
                return;
            }

            if (PlusEnvironment.GetCraftingManager().GetById(recipeId) == null) {
                session.SendWhisper($"No enabled crafting recipe with id {recipeId}.", 1);
                return;
            }

            string username = @params[1];

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(username);
            if (targetClient?.GetHabbo() != null) {
                bool added = targetClient.GetHabbo().GetCraftingRecipes().Unlock(recipeId);
                session.SendWhisper(added ? $"Unlocked recipe {recipeId} for {targetClient.GetHabbo().Username}." : $"{targetClient.GetHabbo().Username} already has recipe {recipeId}.", 1);
                return;
            }

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            int userId = db.Users.Where(u => u.Username == username).Select(u => u.Id).FirstOrDefault();
            if (userId == 0) {
                session.SendWhisper($"No user named \"{username}\" was found.", 1);
                return;
            }

            if (db.UserCraftingRecipes.Any(r => r.UserId == userId && r.RecipeId == recipeId)) {
                session.SendWhisper($"{username} already has recipe {recipeId}.", 1);
                return;
            }

            db.UserCraftingRecipes.Add(new UserCraftingRecipeEntity { UserId = userId, RecipeId = recipeId, Revealed = false });
            db.SaveChanges();
            session.SendWhisper($"Unlocked recipe {recipeId} for offline user {username}.", 1);
        }
    }
}