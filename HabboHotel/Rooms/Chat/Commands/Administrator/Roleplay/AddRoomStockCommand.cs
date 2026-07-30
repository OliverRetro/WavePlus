using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Stock;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay
{
    internal class AddRoomStockCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_add_stock";

        public string Parameters => "%type% %itemId% %amount% [price]";

        public string Description => "Add stock to this room's pool (types: rp_item, rp_weapon, clothing). A negative amount removes stock; price is per unit.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (room == null) {
                session.SendWhisper("You need to be in a room to stock it.", 1);
                return;
            }

            if (@params.Length < 4) {
                session.SendWhisper("Usage: :addroomstock <rp_item|rp_weapon> <itemId> <amount> [price]", 1);
                return;
            }

            if (!StockCommandUtil.TryParse(session, @params, 1, out string stockType, out int itemId, out int amount, out int? price, out string itemName))
                return;

            RoomStockManager stocks = PlusEnvironment.GetRoomStockManager();
            int newAmount = stocks.AddStock(room.Id, stockType, itemId, amount, RoomStockManager.RoomScope, price);
            int unitPrice = stocks.GetPrice(room.Id, stockType, itemId, RoomStockManager.RoomScope);

            session.SendWhisper($"{(amount > 0 ? "Added" : "Removed")} {Math.Abs(amount)}x {itemName} ({stockType} #{itemId}). This room now stocks {newAmount} at {unitPrice} credits each.", 1);
        }
    }
}