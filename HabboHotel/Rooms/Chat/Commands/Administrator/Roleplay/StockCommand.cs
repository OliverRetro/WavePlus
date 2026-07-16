using System.Collections.Generic;
using System.Text;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.RpItem.Item;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using Plus.HabboHotel.Roleplay.Stock;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay
{
    internal class StockCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_add_stock";

        public string Parameters => "";

        public string Description => "Show what this room currently has in stock.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (room == null)
            {
                session.SendWhisper("You need to be in a room to check its stock.", 1);
                return;
            }

            List<(string StockType, int ItemId, int Amount)> stock = PlusEnvironment.GetRoomStockManager().GetRoomStock(room.Id);

            if (stock.Count == 0)
            {
                session.SendWhisper("This room has no stock. Add some with :addstock.", 1);
                return;
            }

            StringBuilder message = new();
            message.AppendLine($"Stock in {room.Name}:");

            foreach ((string stockType, int itemId, int amount) in stock)
                message.AppendLine($"{Describe(stockType, itemId)} ({stockType} #{itemId}): {amount}");

            session.SendNotification(message.ToString());
        }

        private static string Describe(string stockType, int itemId)
        {
            if (stockType == StockType.RpItem && PlusEnvironment.GetRpItemManager().TryGetItem(itemId, out RpItemData item))
                return item.Name;

            if (stockType == StockType.RpWeapon && PlusEnvironment.GetWeaponManager().TryGetWeapon(itemId, out Weapon weapon))
                return weapon.Name;

            return $"#{itemId}";
        }
    }
}
