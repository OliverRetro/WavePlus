using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Marketplace;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Incoming.Marketplace
{
    internal class CancelOfferEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null)
                return;

            int offerId = packet.PopInt();
            uint offerIdU = (uint)offerId;

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            var offerData = db.CatalogMarketplaceOffers.Where(o => o.OfferId == offerIdU)
                .Select(o => new { o.UserId, o.ItemId, o.ExtraData, o.FurniId, o.LimitedNumber, o.LimitedStack })
                .FirstOrDefault();

            if (offerData == null) {
                session.SendPacket(new MarketplaceCancelOfferResultComposer(offerId, false));
                return;
            }

            if (offerData.UserId != session.GetHabbo().Id) {
                session.SendPacket(new MarketplaceCancelOfferResultComposer(offerId, false));
                return;
            }

            if (!PlusEnvironment.GetGame().GetItemManager().GetItem((int)offerData.ItemId, out ItemData item)) {
                session.SendPacket(new MarketplaceCancelOfferResultComposer(offerId, false));
                return;
            }

            Item giveItem = ItemFactory.CreateSingleItem(item, session.GetHabbo(), offerData.ExtraData, offerData.ExtraData, (int)offerData.FurniId, offerData.LimitedNumber, offerData.LimitedStack);
            session.SendPacket(new FurniListNotificationComposer(giveItem.Id, 1));
            session.SendPacket(new FurniListUpdateComposer());

            uint ownerId = (uint)session.GetHabbo().Id;
            db.CatalogMarketplaceOffers.Where(o => o.OfferId == offerIdU && o.UserId == ownerId).ExecuteDelete();

            session.GetHabbo().GetInventoryComponent().UpdateItems(true);
            session.SendPacket(new MarketplaceCancelOfferResultComposer(offerId, true));
        }
    }
}