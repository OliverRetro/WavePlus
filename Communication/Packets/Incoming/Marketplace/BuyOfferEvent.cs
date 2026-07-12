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
            var row = db.catalog_marketplace_offers.Where(o => o.offer_id == offerIdU)
                .Select(o => new { o.state, o.timestamp, o.total_price, o.extra_data, o.item_id, o.furni_id, o.user_id, o.limited_number, o.limited_stack })
                .FirstOrDefault();

            if (row == null) {
                ReloadOffers(session);
                return;
            }

            if (row.state == "2") {
                session.SendNotification("Oops, this offer is no longer available.");
                ReloadOffers(session);
                return;
            }

            if (PlusEnvironment.GetGame().GetCatalog().GetMarketplace().FormatTimestamp() > row.timestamp) {
                session.SendNotification("Oops, this offer has expired..");
                ReloadOffers(session);
                return;
            }

            if (!PlusEnvironment.GetGame().GetItemManager().GetItem((int)row.item_id, out ItemData item)) {
                session.SendNotification("Item isn't in the hotel anymore.");
                ReloadOffers(session);
                return;
            }

            if (row.user_id == session.GetHabbo().Id) {
                session.SendNotification("To prevent average boosting you cannot purchase your own marketplace offers.");
                return;
            }

            if (row.total_price > session.GetHabbo().Credits) {
                session.SendNotification("Oops, you do not have enough credits for this.");
                return;
            }

            session.GetHabbo().Credits -= row.total_price;
            session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));


            Item giveItem = ItemFactory.CreateSingleItem(item, session.GetHabbo(), row.extra_data, row.extra_data, (int)row.furni_id, row.limited_number, row.limited_stack);
            if (giveItem != null) {
                session.GetHabbo().GetInventoryComponent().TryAddItem(giveItem);
                session.SendPacket(new FurniListNotificationComposer(giveItem.Id, 1));

                session.SendPacket(new PurchaseOkComposer());
                session.SendPacket(new FurniListAddComposer(giveItem));
                session.SendPacket(new FurniListUpdateComposer());
            }


            {
                int spriteId = item.SpriteId;
                int salePrice = row.total_price;
                db.catalog_marketplace_offers.Where(o => o.offer_id == offerIdU).ExecuteUpdate(s => s.SetProperty(o => o.state, "2"));

                int id = db.catalog_marketplace_data.Where(d => d.sprite == spriteId).Select(d => d.id).FirstOrDefault();

                if (id > 0)
                    db.catalog_marketplace_data.Where(d => d.id == id).ExecuteUpdate(s => s
                        .SetProperty(d => d.sold, d => d.sold + 1)
                        .SetProperty(d => d.avgprice, d => d.avgprice + salePrice));
                else {
                    db.catalog_marketplace_data.Add(new catalog_marketplace_datum { sprite = spriteId, sold = 1, avgprice = salePrice });
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
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                offers = db.catalog_marketplace_offers
                    .Where(o => o.state == "1" && o.timestamp >= minTimestamp)
                    .OrderByDescending(o => o.asking_price)
                    .Take(500)
                    .Select(o => new { o.offer_id, o.item_type, o.sprite_id, o.total_price, o.limited_number, o.limited_stack })
                    .AsEnumerable()
                    .Select(o => new MarketOffer((int)o.offer_id, o.sprite_id, o.total_price, int.Parse(o.item_type), o.limited_number, o.limited_stack))
                    .ToList();
            }

            PlusEnvironment.GetGame().GetCatalog().GetMarketplace().MarketItems.Clear();
            PlusEnvironment.GetGame().GetCatalog().GetMarketplace().MarketItemKeys.Clear();
            foreach (MarketOffer offer in offers)
            {
                if (!PlusEnvironment.GetGame().GetCatalog().GetMarketplace().MarketItemKeys.Contains(offer.OfferId)) {
                    PlusEnvironment.GetGame().GetCatalog().GetMarketplace().MarketItemKeys.Add(offer.OfferId);
                    PlusEnvironment.GetGame().GetCatalog().GetMarketplace().MarketItems.Add(offer);
                }
            }

            Dictionary<int, MarketOffer> dictionary = new();
            Dictionary<int, int> dictionary2 = new();

            foreach (MarketOffer item in PlusEnvironment.GetGame().GetCatalog().GetMarketplace().MarketItems)
            {
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