using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.RpItem.Item;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
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
            if (room == null)
            {
                session.SendWhisper("You need to be in a room to stock it.", 1);
                return;
            }

            if (@params.Length < 4)
            {
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

    internal class AddItemStockCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_add_stock";

        public string Parameters => "%furniId% %type% %itemId% %amount% [price]";

        public string Description => "Add stock to one stock_vendor furni in this room. A negative amount removes stock; price is per unit.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (room == null)
            {
                session.SendWhisper("You need to be in a room to stock its vendors.", 1);
                return;
            }

            if (@params.Length < 5)
            {
                session.SendWhisper("Usage: :additemstock <furniId> <rp_item|rp_weapon|clothing> <itemId> <amount> [price]", 1);
                return;
            }

            if (!int.TryParse(Convert.ToString(@params[1]), out int furniId) || furniId <= 0)
            {
                session.SendWhisper("Please enter a valid furni ID.", 1);
                return;
            }

            // The furni has to be in this room: stock is keyed by (room, furni), so stocking one
            // that lives elsewhere would create an entry nothing could ever open.
            Items.Item furni = room.GetRoomItemHandler().GetItem(furniId);
            if (furni == null)
            {
                session.SendWhisper($"There's no furni with ID {furniId} in this room.", 1);
                return;
            }

            if (furni.GetBaseItem()?.InteractionType != Items.InteractionType.StockVendor)
            {
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

    /// <summary>Shared argument parsing for the two stock commands.</summary>
    internal static class StockCommandUtil
    {
        /// <summary>
        /// Reads &lt;type&gt; &lt;itemId&gt; &lt;amount&gt; [price] starting at <paramref name="offset"/>.
        /// Price comes back null when omitted, which leaves any existing price untouched rather
        /// than resetting it to zero every time someone restocks.
        /// </summary>
        public static bool TryParse(GameClient session, string[] @params, int offset,
            out string stockType, out int itemId, out int amount, out int? price, out string itemName)
        {
            itemId = 0;
            amount = 0;
            price = null;
            itemName = null;

            stockType = @params[offset].ToLower();
            if (!StockType.IsValid(stockType))
            {
                session.SendWhisper("Unknown stock type. Use rp_item, rp_weapon or clothing.", 1);
                return false;
            }

            if (!int.TryParse(Convert.ToString(@params[offset + 1]), out itemId) || itemId <= 0)
            {
                session.SendWhisper("Please enter a valid item ID.", 1);
                return false;
            }

            if (!int.TryParse(Convert.ToString(@params[offset + 2]), out amount) || amount == 0)
            {
                session.SendWhisper("Please enter a non-zero amount.", 1);
                return false;
            }

            if (@params.Length > offset + 3)
            {
                if (!int.TryParse(Convert.ToString(@params[offset + 3]), out int parsedPrice) || parsedPrice < 0)
                {
                    session.SendWhisper("Please enter a price of 0 or more.", 1);
                    return false;
                }

                price = parsedPrice;
            }

            if (!TryDescribe(stockType, itemId, out itemName))
            {
                session.SendWhisper($"No {stockType} exists with ID {itemId}.", 1);
                return false;
            }

            return true;
        }

        public static bool TryDescribe(string stockType, int itemId, out string itemName)
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
