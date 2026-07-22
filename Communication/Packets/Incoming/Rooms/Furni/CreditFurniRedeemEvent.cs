using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    internal class CreditFurniRedeemEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
                return;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out Room room))
                return;

            if (!room.CheckRights(session, true))
                return;

            if (PlusEnvironment.GetSettingsManager().TryGetValue("room.item.exchangeables.enabled") != "1") {
                session.SendNotification("The hotel managers have temporarilly disabled exchanging!");
                return;
            }

            Item exchange = room.GetRoomItemHandler().GetItem(packet.PopInt());
            if (exchange == null)
                return;

            if (exchange.Data.InteractionType != InteractionType.Exchange)
                return;

            int value = exchange.Data.BehaviourData;

            if (value > 0) {
                session.GetHabbo().Credits += value;
                session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));
            }

            uint exchangeId = (uint)exchange.Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.Items.Where(i => i.Id == exchangeId).ExecuteDelete();
            }

            session.SendPacket(new FurniListUpdateComposer());
            room.GetRoomItemHandler().RemoveFurniture(null, exchange.Id);
            session.GetHabbo().GetInventoryComponent().RemoveItem(exchange.Id);
        }
    }
}