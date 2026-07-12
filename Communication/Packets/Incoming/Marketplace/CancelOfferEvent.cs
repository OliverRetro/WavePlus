using System;
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
            var offerData = db.catalog_marketplace_offers.Where(o => o.offer_id == offerIdU)
                .Select(o => new { o.user_id, o.item_id, o.extra_data, o.furni_id, o.limited_number, o.limited_stack })
                .FirstOrDefault();

            if (offerData == null) {
                session.SendPacket(new MarketplaceCancelOfferResultComposer(offerId, false));
                return;
            }

            if (offerData.user_id != session.GetHabbo().Id) {
                session.SendPacket(new MarketplaceCancelOfferResultComposer(offerId, false));
                return;
            }

            if (!PlusEnvironment.GetGame().GetItemManager().GetItem((int)offerData.item_id, out ItemData item)) {
                session.SendPacket(new MarketplaceCancelOfferResultComposer(offerId, false));
                return;
            }

            Item giveItem = ItemFactory.CreateSingleItem(item, session.GetHabbo(), offerData.extra_data, offerData.extra_data, (int)offerData.furni_id, offerData.limited_number, offerData.limited_stack);
            session.SendPacket(new FurniListNotificationComposer(giveItem.Id, 1));
            session.SendPacket(new FurniListUpdateComposer());

            uint ownerId = (uint)session.GetHabbo().Id;
            db.catalog_marketplace_offers.Where(o => o.offer_id == offerIdU && o.user_id == ownerId).ExecuteDelete();

            session.GetHabbo().GetInventoryComponent().UpdateItems(true);
            session.SendPacket(new MarketplaceCancelOfferResultComposer(offerId, true));
        }
    }
}