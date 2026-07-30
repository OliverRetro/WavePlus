using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay
{
    internal class UnlockCorpRecipeCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_unlock_corp_recipe";

        public string Parameters => "%corpId% %recipeId%";

        public string Description => "Unlock a crafting recipe for a corporation (group).";

        public bool UsableWhileDead => true;

        public bool UsableWhileCuffed => true;

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 3 || !int.TryParse(@params[1], out int corpId) || !int.TryParse(@params[2], out int recipeId)) {
                session.SendWhisper("Usage: :unlockcorprecipe <corpId> <recipeId>", 1);
                return;
            }

            if (PlusEnvironment.GetCraftingManager().GetById(recipeId) == null) {
                session.SendWhisper($"No enabled crafting recipe with id {recipeId}.", 1);
                return;
            }

            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(corpId, out Group group)) {
                session.SendWhisper($"No corporation/group with id {corpId}.", 1);
                return;
            }

            bool added = PlusEnvironment.GetCraftingManager().UnlockGroup(corpId, recipeId);
            session.SendWhisper(added ? $"Unlocked recipe {recipeId} for corporation {group.Name} (#{corpId})." : $"Corporation {group.Name} (#{corpId}) already has recipe {recipeId}.", 1);
        }
    }
}