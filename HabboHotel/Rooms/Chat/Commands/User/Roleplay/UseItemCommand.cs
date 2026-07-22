using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.RpItem.Item;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay
{
    internal class UseItemCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_use_item";

        public string Parameters => "%userItemId%";

        public string Description => "Use one of your RP items.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 2 || !int.TryParse(Convert.ToString(@params[1]), out int userItemId)) {
                session.SendWhisper("Please enter a valid item ID.", 1);
                return;
            }

            UserRpItems items = session.GetHabbo()?.GetRpItems();
            UserRpItem item = items?.GetItem(userItemId);
            if (item == null) {
                session.SendWhisper("You don't own that item.", 1);
                return;
            }

            if (!items.Use(userItemId, session.GetHabbo()))
                session.SendWhisper("You couldn't use that item right now.", 1);
        }
    }
}