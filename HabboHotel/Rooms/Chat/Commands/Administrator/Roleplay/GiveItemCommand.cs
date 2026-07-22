using System;
using System.Linq;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.RpItem;
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
            if (@params.Length < 2 || !int.TryParse(Convert.ToString(@params[2]), out int itemId)) {
                session.SendWhisper("Please enter a valid item ID.", 1);
                return;
            }

            GameClient targetClient = session;
            if (@params.Length >= 3)
                targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);

            if (targetClient?.GetHabbo() == null) {
                session.SendWhisper("That user could not be found.", 1);
                return;
            }

            var targetHabbo = targetClient.GetHabbo();
            UserRpItems targetItems = targetHabbo.GetRpItems();

            // Enforce a cap of 10 distinct inventory stacks. Adding an item that merges into an
            // existing, non-full stack of the same id doesn't consume a new slot — so it's allowed.
            if (targetItems != null && PlusEnvironment.GetRpItemManager().TryGetItem(itemId, out RpItemData def)) {
                bool mergesIntoStack = def.IsStackable &&
                    targetItems.GetItemsByItemId(itemId).Count() % def.StackLimit != 0;

                if (!mergesIntoStack && RpInventory.UsedSlots(targetHabbo, 10) >= 10) {
                    session.SendWhisper($"{targetHabbo.Username}'s inventory is full (10 stacks max).", 1);
                    return;
                }
            }

            UserRpItem item = targetItems?.AddItem(itemId);
            if (item == null) {
                session.SendWhisper("That item ID does not exist.", 1);
                return;
            }

            targetClient.GetHabbo().SaveRpItems();

            // Live-refresh the recipient's inventory panel if they have it open.
            Plus.Communication.Packets.Incoming.WebOverlayCallbackEvent.RefreshInventory(targetClient);

            session.SendWhisper($"Gave {item.ItemData.Name} (#{item.Id}) to {targetClient.GetHabbo().Username}.", 1);
        }
    }
}