using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Marketplace;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.Catalog.Marketplace;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Incoming.Marketplace
{
    internal class BuyOfferEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int offerId = packet.PopInt();
            uint offerIdU = (uint)offerId;

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            var row = db.CatalogMarketplaceOffers.Where(o => o.OfferId == offerIdU)
                .Select(o => new { o.State, o.Timestamp, o.TotalPrice, o.ExtraData, o.ItemId, o.FurniId, o.UserId, o.LimitedNumber, o.LimitedStack })
                .FirstOrDefault();

            if (row == null) {
                ReloadOffers(session);
                return;
            }

            if (row.State == "2") {
                session.SendNotification("Oops, this offer is no longer available.");
                ReloadOffers(session);
                return;
            }

            if (PlusEnvironment.GetGame().GetCatalog().GetMarketplace().FormatTimestamp() > row.Timestamp) {
                session.SendNotification("Oops, this offer has expired..");
                ReloadOffers(session);
                return;
            }

            if (!PlusEnvironment.GetGame().GetItemManager().GetItem((int)row.ItemId, out ItemData item)) {
                session.SendNotification("Item isn't in the hotel anymore.");
                ReloadOffers(session);
                return;
            }

            if (row.UserId == session.GetHabbo().Id) {
                session.SendNotification("To prevent average boosting you cannot purchase your own marketplace offers.");
                return;
            }

            if (row.TotalPrice > session.GetHabbo().Credits) {
                session.SendNotification("Oops, you do not have enough credits for this.");
                return;
            }

            session.GetHabbo().Credits -= row.TotalPrice;
            session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));

            HabboHotel.Items.Item giveItem = ItemFactory.CreateSingleItem(item, session.GetHabbo(), row.ExtraData, row.ExtraData, (int)row.FurniId, row.LimitedNumber, row.LimitedStack);
            if (giveItem != null) {
                session.GetHabbo().GetInventoryComponent().TryAddItem(giveItem);
                session.SendPacket(new FurniListNotificationComposer(giveItem.Id, 1));

                session.SendPacket(new PurchaseOkComposer());
                session.SendPacket(new FurniListAddComposer(giveItem));
                session.SendPacket(new FurniListUpdateComposer());
            }

            {
                int spriteId = item.SpriteId;
                int salePrice = row.TotalPrice;
                db.CatalogMarketplaceOffers.Where(o => o.OfferId == offerIdU).ExecuteUpdate(s => s.SetProperty(o => o.State, "2"));

                int id = db.CatalogMarketplaceData.Where(d => d.Sprite == spriteId).Select(d => d.Id).FirstOrDefault();

                if (id > 0)
                    db.CatalogMarketplaceData.Where(d => d.Id == id).ExecuteUpdate(s => s
                        .SetProperty(d => d.Sold, d => d.Sold + 1)
                        .SetProperty(d => d.Avgprice, d => d.Avgprice + salePrice));
                else {
                    db.CatalogMarketplaceData.Add(new CatalogMarketplaceDatumEntity { Sprite = spriteId, Sold = 1, Avgprice = salePrice });
                    db.SaveChanges();
                }

                if (PlusEnvironment.GetGame().GetCatalog().GetMarketplace().MarketAverages.ContainsKey(item.SpriteId) && PlusEnvironment.GetGame().GetCatalog().GetMarketplace().MarketCounts.ContainsKey(item.SpriteId)) {
                    int num3 = PlusEnvironment.GetGame().GetCatalog().GetMarketplace().MarketCounts[item.SpriteId];
                    int num4 = (PlusEnvironment.GetGame().GetCatalog().GetMarketplace().MarketAverages[item.SpriteId] += salePrice);

                    PlusEnvironment.GetGame().GetCatalog().GetMarketplace().MarketAverages.Remove(item.SpriteId);
                    PlusEnvironment.GetGame().GetCatalog().GetMarketplace().MarketAverages.Add(item.SpriteId, num4);
                    PlusEnvironment.GetGame().GetCatalog().GetMarketplace().MarketCounts.Remove(item.SpriteId);
                    PlusEnvironment.GetGame().GetCatalog().GetMarketplace().MarketCounts.Add(item.SpriteId, num3 + 1);
                } else {
                    if (!PlusEnvironment.GetGame().GetCatalog().GetMarketplace().MarketAverages.ContainsKey(item.SpriteId))
                        PlusEnvironment.GetGame().GetCatalog().GetMarketplace().MarketAverages.Add(item.SpriteId, salePrice);

                    if (!PlusEnvironment.GetGame().GetCatalog().GetMarketplace().MarketCounts.ContainsKey(item.SpriteId))
                        PlusEnvironment.GetGame().GetCatalog().GetMarketplace().MarketCounts.Add(item.SpriteId, 1);
                }
            }

            ReloadOffers(session);
        }

        private void ReloadOffers(GameClient session)
        {
            double minTimestamp = PlusEnvironment.GetGame().GetCatalog().GetMarketplace().FormatTimestamp();

            List<MarketOffer> offers;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                offers = db.CatalogMarketplaceOffers
                    .Where(o => o.State == "1" && o.Timestamp >= minTimestamp)
                    .OrderByDescending(o => o.AskingPrice)
                    .Take(500)
                    .Select(o => new { o.OfferId, o.ItemType, o.SpriteId, o.TotalPrice, o.LimitedNumber, o.LimitedStack })
                    .AsEnumerable()
                    .Select(o => new MarketOffer((int)o.OfferId, o.SpriteId, o.TotalPrice, int.Parse(o.ItemType), o.LimitedNumber, o.LimitedStack))
                    .ToList();
            }

            PlusEnvironment.GetGame().GetCatalog().GetMarketplace().MarketItems.Clear();
            PlusEnvironment.GetGame().GetCatalog().GetMarketplace().MarketItemKeys.Clear();
            foreach (MarketOffer offer in offers) {
                if (!PlusEnvironment.GetGame().GetCatalog().GetMarketplace().MarketItemKeys.Contains(offer.OfferId)) {
                    PlusEnvironment.GetGame().GetCatalog().GetMarketplace().MarketItemKeys.Add(offer.OfferId);
                    PlusEnvironment.GetGame().GetCatalog().GetMarketplace().MarketItems.Add(offer);
                }
            }

            Dictionary<int, MarketOffer> dictionary = new();
            Dictionary<int, int> dictionary2 = new();

            foreach (MarketOffer item in PlusEnvironment.GetGame().GetCatalog().GetMarketplace().MarketItems) {
                if (dictionary.ContainsKey(item.SpriteId)) {
                    if (dictionary[item.SpriteId].TotalPrice > item.TotalPrice) {
                        dictionary.Remove(item.SpriteId);
                        dictionary.Add(item.SpriteId, item);
                    }

                    int num = dictionary2[item.SpriteId];
                    dictionary2.Remove(item.SpriteId);
                    dictionary2.Add(item.SpriteId, num + 1);
                } else {
                    dictionary.Add(item.SpriteId, item);
                    dictionary2.Add(item.SpriteId, 1);
                }
            }

            session.SendPacket(new MarketPlaceOffersComposer(dictionary, dictionary2));
        }
    }
}