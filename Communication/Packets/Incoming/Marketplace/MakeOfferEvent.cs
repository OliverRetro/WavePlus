using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Marketplace;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.Catalog.Utilities;
using Plus.HabboHotel.GameClients;
// Disambiguates the bare name from the scaffolded EF entity Plus.Database.EF.Entities.Item.
using Item = Plus.HabboHotel.Items.Item;

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
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.CatalogMarketplaceOffers.Add(new CatalogMarketplaceOfferEntity
                {
                    FurniId = itemIdU,
                    ItemId = (uint)item.BaseItem,
                    UserId = (uint)ownerId,
                    AskingPrice = sellingPrice,
                    TotalPrice = totalPrice,
                    PublicName = item.GetBaseItem().PublicName,
                    SpriteId = item.GetBaseItem().SpriteId,
                    ItemType = itemType.ToString(),
                    Timestamp = PlusEnvironment.GetUnixTimestamp(),
                    ExtraData = item.ExtraData,
                    LimitedNumber = item.LimitedNo,
                    LimitedStack = item.LimitedTot
                });
                db.SaveChanges();

                db.Items.Where(i => i.Id == itemIdU && i.UserId == ownerId).ExecuteDelete();
            }

            session.GetHabbo().GetInventoryComponent().RemoveItem(itemId);
            session.SendPacket(new MarketplaceMakeOfferResultComposer(1));
        }
    }
}