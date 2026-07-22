using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Marketplace;
using Plus.Database.EF;
using Plus.HabboHotel.Catalog.Marketplace;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Marketplace
{
    internal class GetOffersEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int minCost = packet.PopInt();
            int maxCost = packet.PopInt();
            string searchQuery = packet.PopString();
            int filterMode = packet.PopInt();

            double minTimestamp = PlusEnvironment.GetGame().GetCatalog().GetMarketplace().FormatTimestamp();

            List<MarketOffer> offers;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var query = db.CatalogMarketplaceOffers.Where(o => o.State == "1" && o.Timestamp >= minTimestamp);
                if (minCost >= 0)
                    query = query.Where(o => o.TotalPrice > minCost);
                if (maxCost >= 0)
                    query = query.Where(o => o.TotalPrice < maxCost);
                if (searchQuery.Length >= 1)
                    query = query.Where(o => EF.Functions.Like(o.PublicName, "%" + searchQuery + "%"));

                query = filterMode == 1
                    ? query.OrderByDescending(o => o.AskingPrice)
                    : query.OrderBy(o => o.AskingPrice);

                offers = query.Take(500)
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
                    if (item.LimitedNumber > 0) {
                        if (!dictionary.ContainsKey(item.OfferId))
                            dictionary.Add(item.OfferId, item);
                        if (!dictionary2.ContainsKey(item.OfferId))
                            dictionary2.Add(item.OfferId, 1);
                    } else {
                        if (dictionary[item.SpriteId].TotalPrice > item.TotalPrice) {
                            dictionary.Remove(item.SpriteId);
                            dictionary.Add(item.SpriteId, item);
                        }

                        int num = dictionary2[item.SpriteId];
                        dictionary2.Remove(item.SpriteId);
                        dictionary2.Add(item.SpriteId, num + 1);
                    }
                } else {
                    if (!dictionary.ContainsKey(item.SpriteId))
                        dictionary.Add(item.SpriteId, item);
                    if (!dictionary2.ContainsKey(item.SpriteId))
                        dictionary2.Add(item.SpriteId, 1);
                }
            }

            session.SendPacket(new MarketPlaceOffersComposer(dictionary, dictionary2));
        }
    }
}