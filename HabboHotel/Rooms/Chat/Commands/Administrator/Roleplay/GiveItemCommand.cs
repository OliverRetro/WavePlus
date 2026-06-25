using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.RpItem.Item;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay
{
    internal class GiveItemCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_give_item";

        public string Parameters => "%username% %itemId%";

        public string Description => "Give an RP item to yourself or another user.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 2 || !int.TryParse(Convert.ToString(@params[2]), out int itemId))
            {
                session.SendWhisper("Please enter a valid item ID.", 1);
                return;
            }

            GameClient targetClient = session;
            if (@params.Length >= 3)
                targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);

            if (targetClient?.GetHabbo() == null)
            {
                session.SendWhisper("That user could not be found.", 1);
                return;
            }

            UserRpItem item = targetClient.GetHabbo().GetRpItems()?.AddItem(itemId);
            if (item == null)
            {
                session.SendWhisper("That item ID does not exist.", 1);
                return;
            }

            targetClient.GetHabbo().SaveRpItems();

            session.SendWhisper($"Gave {item.ItemData.Name} (#{item.Id}) to {targetClient.GetHabbo().Username}.", 1);
        }
    }
}
