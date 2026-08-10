using Plus.Communication.Packets.Outgoing.Overlay;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.RpItem;
using Plus.HabboHotel.Roleplay.RpItem.Item;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using Plus.HabboHotel.Users;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Plus.HabboHotel.Roleplay.Stock
{
    public static class RpOfferService
    {
        public const int CommissionPercent = 5;

        public const int ExpirySeconds = 180;

        private const int InventorySlots = 10;

        public sealed class PendingOffer
        {
            public int Id { get; init; }
            public int RoomId { get; init; }
            public int FurniId { get; init; }

            public int SellerId { get; init; }
            public string SellerName { get; init; }

            public string Title { get; init; }
            public DateTime SentAt { get; init; }

            public bool IsExpired => (DateTime.UtcNow - SentAt).TotalSeconds > ExpirySeconds;
        }

        // Keyed by buyer: a user only ever has one buy window on screen, so a second offer
        // replaces whatever was already there.
        private static readonly ConcurrentDictionary<int, PendingOffer> _pending = new();
        private static int _nextId;

        public static void Clear(int userId) => _pending.TryRemove(userId, out _);

        public static bool Open(GameClient buyer, int roomId, int furniId, Habbo seller, string title)
        {
            Habbo habbo = buyer?.GetHabbo();
            if (habbo == null)
                return false;

            List<(string StockType, int ItemId, int Amount, int Price)> stock =
                PlusEnvironment.GetRoomStockManager().GetRoomStock(roomId, furniId);

            if (stock.Count == 0)
                return false;

            PendingOffer offer = new()
            {
                Id = System.Threading.Interlocked.Increment(ref _nextId),
                RoomId = roomId,
                FurniId = furniId,
                SellerId = seller?.Id ?? 0,
                SellerName = seller?.Username ?? "",
                Title = title,
                SentAt = DateTime.UtcNow
            };

            _pending[habbo.Id] = offer;

            SendWindow(buyer, offer, stock, "");
            return true;
        }

        private static void Refresh(GameClient buyer, PendingOffer offer, string message)
        {
            SendWindow(buyer, offer,
                PlusEnvironment.GetRoomStockManager().GetRoomStock(offer.RoomId, offer.FurniId), message);
        }

        private static void SendWindow(GameClient buyer, PendingOffer offer,
            List<(string StockType, int ItemId, int Amount, int Price)> stock, string message)
        {
            Habbo habbo = buyer.GetHabbo();

            List<object> entries = new();
            foreach ((string stockType, int itemId, int amount, int price) in stock) {
                if (!TryDescribe(stockType, itemId, out string name, out string image, out int rarity))
                    continue;

                entries.Add(new
                {
                    stockType,
                    itemId,
                    name,
                    image,
                    rarity,
                    price,
                    stock = amount
                });
            }

            WebOverlay.Send(buyer, "pre_offer", new
            {
                offerId = offer.Id,
                title = offer.Title,
                sellerName = offer.SellerName,
                // Vendor furni sales pay nobody, so the window shouldn't advertise a commission.
                commissionPercent = offer.SellerId == 0 ? 0 : CommissionPercent,
                credits = habbo.Credits,
                freeSlots = FreeSlots(habbo),
                message,
                items = entries.ToArray()
            });
        }

        public static void HandlePurchase(GameClient buyer, int offerId, string stockType, int itemId, int amount)
        {
            Habbo habbo = buyer?.GetHabbo();
            if (habbo == null)
                return;

            if (!_pending.TryGetValue(habbo.Id, out PendingOffer offer) || offer.Id != offerId)
                return;

            if (offer.IsExpired) {
                _pending.TryRemove(habbo.Id, out _);
                buyer.SendWhisper("That offer has expired.", 1);
                return;
            }

            // The window is tied to the room it was opened in; walking out ends it.
            if (habbo.CurrentRoom == null || habbo.CurrentRoom.Id != offer.RoomId) {
                _pending.TryRemove(habbo.Id, out _);
                buyer.SendWhisper("You've left the room that offer came from.", 1);
                return;
            }

            if (amount <= 0 || !StockType.IsValid(stockType))
                return;

            RoomStockManager stocks = PlusEnvironment.GetRoomStockManager();

            int available = stocks.GetStock(offer.RoomId, stockType, itemId, offer.FurniId);
            if (available <= 0) {
                Refresh(buyer, offer, "That item is out of stock!");
                return;
            }

            amount = Math.Min(amount, available);

            // Clamp to what actually fits. Reported afterwards so the buyer knows why they got
            // fewer than they asked for.
            int fits = Capacity(habbo, stockType, itemId, amount);
            bool clamped = fits < amount;
            amount = fits;

            if (amount <= 0) {
                Refresh(buyer, offer, "Your inventory is full.");
                return;
            }

            int unitPrice = stocks.GetPrice(offer.RoomId, stockType, itemId, offer.FurniId);
            int total = unitPrice * amount;

            if (total > habbo.Credits) {
                int affordable = unitPrice <= 0 ? amount : habbo.Credits / unitPrice;
                Refresh(buyer, offer,
                    affordable > 0
                        ? $"You can't afford {amount} of those — you can afford {affordable}."
                        : "You can't afford that.");
                return;
            }

            // Consume last, once every other check has passed, so a failure can't eat stock.
            if (!stocks.TryConsume(offer.RoomId, stockType, itemId, amount, offer.FurniId)) {
                Refresh(buyer, offer, "Someone beat you to the last of that stock!");
                return;
            }

            if (!Deliver(habbo, stockType, itemId, amount)) {
                // Delivery failed after the stock was taken — put it straight back.
                stocks.AddStock(offer.RoomId, stockType, itemId, amount, offer.FurniId);
                Refresh(buyer, offer, "That item couldn't be delivered.");
                return;
            }

            habbo.Credits -= total;
            WebOverlay.SendCredits(buyer);
            buyer.SendPacket(new Communication.Packets.Outgoing.Inventory.Purse.CreditBalanceComposer(habbo.Credits));

            PayCommission(offer, total);

            habbo.SaveRpItems();
            Communication.Packets.Incoming.WebOverlayCallbackEvent.RefreshInventory(buyer);

            TryDescribe(stockType, itemId, out string boughtName, out _, out _);

            string note = $"Bought {amount}x {boughtName} for {total} credits.";
            if (clamped)
                note += " Your inventory only had room for that many.";

            Refresh(buyer, offer, note);
        }

        private static void PayCommission(PendingOffer offer, int total)
        {
            if (offer.SellerId == 0 || total <= 0)
                return;

            int commission = total * CommissionPercent / 100;
            if (commission <= 0)
                return;

            GameClient sellerClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(offer.SellerId);
            Habbo seller = sellerClient?.GetHabbo();
            if (seller == null)
                return;

            seller.Credits += commission;
            WebOverlay.SendCredits(sellerClient);
            sellerClient.SendPacket(new Communication.Packets.Outgoing.Inventory.Purse.CreditBalanceComposer(seller.Credits));
            sellerClient.SendWhisper($"You earnt {commission} credits commission on that sale.", 1);
            seller.GetRpStats().Experience += 2;
        }

        private static int Capacity(Habbo habbo, string stockType, int itemId, int wanted)
        {
            int free = FreeSlots(habbo);

            if (stockType == StockType.RpItem &&
                PlusEnvironment.GetRpItemManager().TryGetItem(itemId, out RpItemData def) && def.IsStackable) {
                // Room left in the partially-filled stack, plus a full stack for every free slot.
                int owned = habbo.GetRpItems()?.GetItemsByItemId(itemId).Count() ?? 0;
                int remainder = owned % def.StackLimit;
                int roomInOpenStack = remainder == 0 ? 0 : def.StackLimit - remainder;

                return Math.Min(wanted, roomInOpenStack + (free * def.StackLimit));
            }

            // Everything else takes one slot per unit.
            return Math.Min(wanted, free);
        }

        private static int FreeSlots(Habbo habbo) =>
            Math.Max(0, InventorySlots - RpInventory.UsedSlots(habbo, InventorySlots));

        private static bool Deliver(Habbo habbo, string stockType, int itemId, int amount)
        {
            switch (stockType) {
                case StockType.RpItem:
                    UserRpItems items = habbo.GetRpItems();
                    if (items == null)
                        return false;

                    for (int i = 0; i < amount; i++) {
                        if (items.AddItem(itemId) == null)
                            return i > 0; // partially delivered is still a sale
                    }

                    return true;

                case StockType.RpWeapon:
                    UserRpWeapons weapons = habbo.GetRpWeapons();
                    if (weapons == null)
                        return false;

                    for (int i = 0; i < amount; i++) {
                        if (weapons.AddWeapon(itemId) == null)
                            return i > 0;
                    }

                    return true;

                default:
                    // Clothing is catalog-side and has no RP inventory representation yet.
                    return false;
            }
        }

        private static bool TryDescribe(string stockType, int itemId, out string name, out string image, out int rarity)
        {
            name = "";
            image = "";
            rarity = 1;

            switch (stockType) {
                case StockType.RpItem:
                    if (!PlusEnvironment.GetRpItemManager().TryGetItem(itemId, out RpItemData item))
                        return false;

                    name = item.Name ?? "";
                    image = item.ImageUrl ?? "";
                    rarity = item.Rarity;
                    return true;

                case StockType.RpWeapon:
                    if (!PlusEnvironment.GetWeaponManager().TryGetWeapon(itemId, out Weapon weapon))
                        return false;

                    name = weapon.Name ?? "";
                    image = weapon.Image ?? "";
                    rarity = weapon.Rarity < 1 ? 1 : weapon.Rarity;
                    return true;

                default:
                    return false;
            }
        }
    }
}