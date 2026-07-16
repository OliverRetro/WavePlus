using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.RpItem.Item;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using Plus.HabboHotel.Roleplay.Stock;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay
{
    internal class AddStockCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_add_stock";

        public string Parameters => "%type% %itemId% %amount%";

        public string Description => "Add stock of an RP item or weapon to this room (types: rp_item, rp_weapon, clothing). A negative amount removes stock.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (room == null)
            {
                session.SendWhisper("You need to be in a room to stock it.", 1);
                return;
            }

            if (@params.Length < 4)
            {
                session.SendWhisper("Usage: :addstock <rp_item|rp_weapon|clothing> <itemId> <amount>", 1);
                return;
            }

            string stockType = @params[1].ToLower();
            if (!StockType.IsValid(stockType))
            {
                session.SendWhisper("Unknown stock type. Use rp_item, rp_weapon or clothing.", 1);
                return;
            }

            if (!int.TryParse(Convert.ToString(@params[2]), out int itemId) || itemId <= 0)
            {
                session.SendWhisper("Please enter a valid item ID.", 1);
                return;
            }

            if (!int.TryParse(Convert.ToString(@params[3]), out int amount) || amount == 0)
            {
                session.SendWhisper("Please enter a non-zero amount.", 1);
                return;
            }

            if (!TryDescribe(stockType, itemId, out string itemName))
            {
                session.SendWhisper($"No {stockType} exists with ID {itemId}.", 1);
                return;
            }

            int newAmount = PlusEnvironment.GetRoomStockManager().AddStock(room.Id, stockType, itemId, amount);
            session.SendWhisper($"{(amount > 0 ? "Added" : "Removed")} {Math.Abs(amount)}x {itemName} ({stockType} #{itemId}). This room now stocks {newAmount}.", 1);
        }

        private static bool TryDescribe(string stockType, int itemId, out string itemName)
        {
            itemName = null;

            switch (stockType)
            {
                case StockType.RpItem:
                    if (!PlusEnvironment.GetRpItemManager().TryGetItem(itemId, out RpItemData item))
                        return false;
                    itemName = item.Name;
                    return true;

                case StockType.RpWeapon:
                    if (!PlusEnvironment.GetWeaponManager().TryGetWeapon(itemId, out Weapon weapon))
                        return false;
                    itemName = weapon.Name;
                    return true;

                default:
                    // Clothing ids are catalog-side; accept them as-is for now.
                    itemName = $"clothing #{itemId}";
                    return true;
            }
        }
    }
}
