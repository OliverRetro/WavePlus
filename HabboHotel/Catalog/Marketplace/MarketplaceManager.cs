using System;
using System.Collections.Generic;
using System.Linq;
using Plus.Database.EF;

namespace Plus.HabboHotel.Catalog.Marketplace
{
    public class MarketplaceManager
    {
        public List<int> MarketItemKeys = new();
        public List<MarketOffer> MarketItems = new();
        public Dictionary<int, int> MarketCounts = new();
        public Dictionary<int, int> MarketAverages = new();

        public int AvgPriceForSprite(int spriteId)
        {
            int num = 0;
            int num2 = 0;
            if (MarketAverages.ContainsKey(spriteId) && MarketCounts.ContainsKey(spriteId)) {
                if (MarketCounts[spriteId] > 0) {
                    return (MarketAverages[spriteId] / MarketCounts[spriteId]);
                }

                return 0;
            }

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var data = db.CatalogMarketplaceData.Where(d => d.Sprite == spriteId).Select(d => new { d.Avgprice, d.Sold }).FirstOrDefault();
                if (data != null) {
                    num = data.Avgprice;
                    num2 = data.Sold;
                }
            }

            MarketAverages.Add(spriteId, num);
            MarketCounts.Add(spriteId, num2);

            if (num2 > 0)
                return Convert.ToInt32(Math.Ceiling((double)(num / num2)));

            return 0;
        }

        public string FormatTimestampString()
        {
            return FormatTimestamp().ToString().Split(new[] { ',' })[0];
        }

        public double FormatTimestamp()
        {
            return PlusEnvironment.GetUnixTimestamp() - 172800.0;
        }

        public int OfferCountForSprite(int spriteId)
        {
            Dictionary<int, MarketOffer> dictionary = new();
            Dictionary<int, int> dictionary2 = new();
            foreach (MarketOffer item in MarketItems) {
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

            if (dictionary2.ContainsKey(spriteId)) {
                return dictionary2[spriteId];
            }

            return 0;
        }

        public int CalculateCommissionPrice(float price)
        {
            return Convert.ToInt32(Math.Ceiling(price / 100 * 1));
        }
    }
}