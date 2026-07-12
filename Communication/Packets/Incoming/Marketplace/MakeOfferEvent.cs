using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Marketplace;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.Catalog.Utilities;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Incoming.Marketplace
{
    internal class MakeOfferEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int sellingPrice = packet.PopInt();
            packet.PopInt(); //comission
            int itemId = packet.PopInt();

            Item item = session.GetHabbo().GetInventoryComponent().GetItem(itemId);
            if (item == null) {
                session.SendPacket(new MarketplaceMakeOfferResultComposer(0));
                return;
            }

            if (!ItemUtility.IsRare(item)) {
                session.SendNotification("Sorry, only Rares & LTDs can go be auctioned off in the Marketplace!");
                return;
            }

            if (sellingPrice > 70000000 || sellingPrice == 0) {
                session.SendPacket(new MarketplaceMakeOfferResultComposer(0));
                return;
            }

            int commission = PlusEnvironment.GetGame().GetCatalog().GetMarketplace().CalculateCommissionPrice(sellingPrice);
            int totalPrice = sellingPrice + commission;
            int itemType = 1;
            if (item.GetBaseItem().Type == 'i')
                itemType++;

            int ownerId = session.GetHabbo().Id;
            uint itemIdU = (uint)itemId;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                db.catalog_marketplace_offers.Add(new catalog_marketplace_offer
                {
                    furni_id = itemIdU,
                    item_id = (uint)item.BaseItem,
                    user_id = (uint)ownerId,
                    asking_price = sellingPrice,
                    total_price = totalPrice,
                    public_name = item.GetBaseItem().PublicName,
                    sprite_id = item.GetBaseItem().SpriteId,
                    item_type = itemType.ToString(),
                    timestamp = PlusEnvironment.GetUnixTimestamp(),
                    extra_data = item.ExtraData,
                    limited_number = item.LimitedNo,
                    limited_stack = item.LimitedTot
                });
                db.SaveChanges();

                db.items.Where(i => i.id == itemIdU && i.user_id == ownerId).ExecuteDelete();
            }

            session.GetHabbo().GetInventoryComponent().RemoveItem(itemId);
            session.SendPacket(new MarketplaceMakeOfferResultComposer(1));
        }
    }
}