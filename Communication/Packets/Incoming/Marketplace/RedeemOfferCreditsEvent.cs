using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Marketplace
{
    internal class RedeemOfferCreditsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            uint uid = (uint)session.GetHabbo().Id;

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            int creditsOwed = db.catalog_marketplace_offers.Where(o => o.user_id == uid && o.state == "2").Sum(o => (int?)o.asking_price) ?? 0;

            if (creditsOwed >= 1) {
                session.GetHabbo().Credits += creditsOwed;
                session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));

                db.catalog_marketplace_offers.Where(o => o.user_id == uid && o.state == "2").ExecuteDelete();
            }
        }
    }
}