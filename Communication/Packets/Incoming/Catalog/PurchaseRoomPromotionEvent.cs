using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users.Messenger;
// Bare RoomPromotion = the gameplay type; the EF entity is fully-qualified at its DB upsert site.
using RoomPromotion = Plus.HabboHotel.Rooms.RoomPromotion;

namespace Plus.Communication.Packets.Incoming.Catalog
{
    public class PurchaseRoomPromotionEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null)
                return;

            packet.PopInt(); //pageId
            packet.PopInt(); //itemId
            int roomId = packet.PopInt();
            string name = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(packet.PopString());
            packet.PopBoolean(); //junk
            string desc = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(packet.PopString());
            int categoryId = packet.PopInt();

            if (!RoomFactory.TryGetData(roomId, out RoomData data))
                return;

            if (data.OwnerId != session.GetHabbo().Id)
                return;

            if (data.Promotion == null)
                data.Promotion = new RoomPromotion(name, desc, categoryId);
            else {
                data.Promotion.Name = name;
                data.Promotion.Description = desc;
                data.Promotion.TimestampExpires += 7200;
            }

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.RoomPromotions.Upsert(new Plus.Database.EF.Entities.RoomPromotionEntity
                {
                    RoomId = roomId,
                    Title = name,
                    Description = desc,
                    TimestampStart = data.Promotion.TimestampStarted,
                    TimestampExpire = data.Promotion.TimestampExpires,
                    CategoryId = categoryId
                }).Run();
            }

            if (!session.GetHabbo().GetBadgeComponent().HasBadge("RADZZ"))
                session.GetHabbo().GetBadgeComponent().GiveBadge("RADZZ", true, session);

            session.SendPacket(new PurchaseOkComposer());
            if (session.GetHabbo().InRoom && session.GetHabbo().CurrentRoomId == roomId)
                session.GetHabbo().CurrentRoom.SendPacket(new RoomEventComposer(data, data.Promotion));

            session.GetHabbo().GetMessenger().BroadcastAchievement(session.GetHabbo().Id, MessengerEventTypes.EventStarted, name);
        }
    }
}