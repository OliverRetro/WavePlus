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
                avgPrice = db.CatalogMarketplaceData.Where(d => d.Sprite == spriteId).Select(d => d.Avgprice).FirstOrDefault();

            session.SendPacket(new MarketplaceItemStatsComposer(itemId, spriteId, avgPrice));
        }
    }
}