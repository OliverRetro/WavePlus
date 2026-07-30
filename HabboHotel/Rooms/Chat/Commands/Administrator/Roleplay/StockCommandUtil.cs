using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.RpItem.Item;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using Plus.HabboHotel.Roleplay.Stock;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay
{
    internal static class StockCommandUtil
    {
        public static bool TryParse(GameClient session, string[] @params, int offset,
            out string stockType, out int itemId, out int amount, out int? price, out string itemName)
        {
            itemId = 0;
            amount = 0;
            price = null;
            itemName = null;

            stockType = @params[offset].ToLower();
            if (!StockType.IsValid(stockType)) {
                session.SendWhisper("Unknown stock type. Use rp_item, rp_weapon or clothing.", 1);
                return false;
            }

            if (!int.TryParse(Convert.ToString(@params[offset + 1]), out itemId) || itemId <= 0) {
                session.SendWhisper("Please enter a valid item ID.", 1);
                return false;
            }

            if (!int.TryParse(Convert.ToString(@params[offset + 2]), out amount) || amount == 0) {
                session.SendWhisper("Please enter a non-zero amount.", 1);
                return false;
            }

            if (@params.Length > offset + 3) {
                if (!int.TryParse(Convert.ToString(@params[offset + 3]), out int parsedPrice) || parsedPrice < 0) {
                    session.SendWhisper("Please enter a price of 0 or more.", 1);
                    return false;
                }

                price = parsedPrice;
            }

            if (!TryDescribe(stockType, itemId, out itemName)) {
                session.SendWhisper($"No {stockType} exists with ID {itemId}.", 1);
                return false;
            }

            return true;
        }

        public static bool TryDescribe(string stockType, int itemId, out string itemName)
        {
            itemName = null;

            switch (stockType) {
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