using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Roleplay.Stock;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay
{
    internal class AddItemStockCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_add_item_stock";

        public string Parameters => "%furniId% %type% %itemId% %amount% [price]";

        public string Description => "Add stock to one stock_vendor furni in this room. A negative amount removes stock; price is per unit.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (room == null) {
                session.SendWhisper("You need to be in a room to stock its vendors.", 1);
                return;
            }

            if (@params.Length < 5) {
                session.SendWhisper("Usage: :additemstock <furniId> <rp_item|rp_weapon|clothing> <itemId> <amount> [price]", 1);
                return;
            }

            if (!int.TryParse(Convert.ToString(@params[1]), out int furniId) || furniId <= 0) {
                session.SendWhisper("Please enter a valid furni ID.", 1);
                return;
            }

            Item furni = room.GetRoomItemHandler().GetItem(furniId);
            if (furni == null) {
                session.SendWhisper($"There's no furni with ID {furniId} in this room.", 1);
                return;
            }

            if (furni.GetBaseItem()?.InteractionType != Items.InteractionType.StockVendor) {
                session.SendWhisper("That furni isn't a stock vendor.", 1);
                return;
            }

            if (!StockCommandUtil.TryParse(session, @params, 2, out string stockType, out int itemId, out int amount, out int? price, out string itemName))
                return;

            RoomStockManager stocks = PlusEnvironment.GetRoomStockManager();
            int newAmount = stocks.AddStock(room.Id, stockType, itemId, amount, furniId, price);
            int unitPrice = stocks.GetPrice(room.Id, stockType, itemId, furniId);

            session.SendWhisper($"{(amount > 0 ? "Added" : "Removed")} {Math.Abs(amount)}x {itemName} ({stockType} #{itemId}) on vendor #{furniId}. It now stocks {newAmount} at {unitPrice} credits each.", 1);
        }
    }
}