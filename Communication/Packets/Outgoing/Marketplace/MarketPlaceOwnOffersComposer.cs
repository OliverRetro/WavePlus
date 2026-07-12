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
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var offers = db.catalog_marketplace_offers.Where(o => o.user_id == uid)
                    .Select(o => new { o.timestamp, o.state, o.offer_id, o.sprite_id, o.total_price, o.limited_number, o.limited_stack })
                    .ToList();

                int i = db.catalog_marketplace_offers.Where(o => o.state == "2" && o.user_id == uid).Sum(o => (int?)o.asking_price) ?? 0;

                packet.WriteInteger(i);
                packet.WriteInteger(offers.Count);
                foreach (var row in offers)
                {
                    int num2 = Convert.ToInt32(Math.Floor((row.timestamp + 172800.0 - PlusEnvironment.GetUnixTimestamp()) / 60.0));
                    int num3 = int.Parse(row.state);
                    if ((num2 <= 0) && (num3 != 2)) {
                        num3 = 3;
                        num2 = 0;
                    }

                    packet.WriteInteger((int)row.offer_id);
                    packet.WriteInteger(num3);
                    packet.WriteInteger(1);
                    packet.WriteInteger(row.sprite_id);

                    packet.WriteInteger(256);
                    packet.WriteString("");
                    packet.WriteInteger(row.limited_number);
                    packet.WriteInteger(row.limited_stack);

                    packet.WriteInteger(row.total_price);
                    packet.WriteInteger(num2);
                    packet.WriteInteger(row.sprite_id);
                }
            }
        }
    }
}