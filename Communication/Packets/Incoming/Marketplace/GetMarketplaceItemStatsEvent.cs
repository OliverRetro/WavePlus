using System.Linq;
using Plus.Communication.Packets.Outgoing.Marketplace;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Marketplace
{
    internal class GetMarketplaceItemStatsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int itemId = packet.PopInt();
            int spriteId = packet.PopInt();

            int avgPrice;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                avgPrice = db.catalog_marketplace_data.Where(d => d.sprite == spriteId).Select(d => d.avgprice).FirstOrDefault();

            session.SendPacket(new MarketplaceItemStatsComposer(itemId, spriteId, avgPrice));
        }
    }
}