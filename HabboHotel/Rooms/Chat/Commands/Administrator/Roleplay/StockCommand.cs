using System.Collections.Generic;
using System.Text;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Stock;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay
{
    internal class StockCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_get_stock";

        public string Parameters => "";

        public string Description => "Show what this room and its vendors currently have in stock.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (room == null) {
                session.SendWhisper("You need to be in a room to check its stock.", 1);
                return;
            }

            RoomStockManager stocks = PlusEnvironment.GetRoomStockManager();

            List<(string StockType, int ItemId, int Amount, int Price)> roomStock =
                stocks.GetRoomStock(room.Id, RoomStockManager.RoomScope);
            List<int> vendors = stocks.GetStockedFurniIds(room.Id);

            if (roomStock.Count == 0 && vendors.Count == 0) {
                session.SendWhisper("This room has no stock. Add some with :addroomstock or :additemstock.", 1);
                return;
            }

            StringBuilder message = new();
            message.AppendLine($"Stock in {room.Name}:");

            message.AppendLine();
            message.AppendLine("-- Room pool (sold via :offer) --");
            if (roomStock.Count == 0)
                message.AppendLine("(empty)");
            else
                Append(message, roomStock);

            foreach (int furniId in vendors) {
                message.AppendLine();
                message.AppendLine($"-- Vendor #{furniId} --");
                Append(message, stocks.GetRoomStock(room.Id, furniId));
            }

            session.SendNotification(message.ToString());
        }

        private static void Append(StringBuilder message, List<(string StockType, int ItemId, int Amount, int Price)> stock)
        {
            foreach ((string stockType, int itemId, int amount, int price) in stock) {
                StockCommandUtil.TryDescribe(stockType, itemId, out string name);
                message.AppendLine($"{name ?? "#" + itemId} ({stockType} #{itemId}): {amount} @ {price} credits");
            }
        }
    }
}