using System;
using System.Linq;
using Plus.Database.EF;

namespace Plus.Communication.Packets.Outgoing.Marketplace
{
    internal class MarketPlaceOwnOffersComposer : MessageComposer
    {
        public int UserId { get; }

        public MarketPlaceOwnOffersComposer(int userId)
            : base(ServerPacketHeader.MarketPlaceOwnOffersMessageComposer)
        {
            UserId = userId;
        }

        public override void Compose(ServerPacket packet)
        {
            uint uid = (uint)UserId;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var offers = db.CatalogMarketplaceOffers.Where(o => o.UserId == uid)
                    .Select(o => new { o.Timestamp, o.State, o.OfferId, o.SpriteId, o.TotalPrice, o.LimitedNumber, o.LimitedStack })
                    .ToList();

                int i = db.CatalogMarketplaceOffers.Where(o => o.State == "2" && o.UserId == uid).Sum(o => (int?)o.AskingPrice) ?? 0;

                packet.WriteInteger(i);
                packet.WriteInteger(offers.Count);
                foreach (var row in offers) {
                    int num2 = Convert.ToInt32(Math.Floor((row.Timestamp + 172800.0 - PlusEnvironment.GetUnixTimestamp()) / 60.0));
                    int num3 = int.Parse(row.State);
                    if ((num2 <= 0) && (num3 != 2)) {
                        num3 = 3;
                        num2 = 0;
                    }

                    packet.WriteInteger((int)row.OfferId);
                    packet.WriteInteger(num3);
                    packet.WriteInteger(1);
                    packet.WriteInteger(row.SpriteId);

                    packet.WriteInteger(256);
                    packet.WriteString("");
                    packet.WriteInteger(row.LimitedNumber);
                    packet.WriteInteger(row.LimitedStack);

                    packet.WriteInteger(row.TotalPrice);
                    packet.WriteInteger(num2);
                    packet.WriteInteger(row.SpriteId);
                }
            }
        }
    }
}