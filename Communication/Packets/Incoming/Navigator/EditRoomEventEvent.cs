using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Navigator
{
    internal class EditRoomEventEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int roomId = packet.PopInt();
            string name = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(packet.PopString());
            string desc = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(packet.PopString());

            if (!RoomFactory.TryGetData(roomId, out RoomData data))
                return;

            if (data.OwnerId != session.GetHabbo().Id)
                return;

            if (data.Promotion == null) {
                session.SendNotification("Oops, it looks like there isn't a room promotion in this room?");
                return;
            }

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.RoomPromotions.Where(p => p.RoomId == roomId)
                    .ExecuteUpdate(s => s.SetProperty(p => p.Title, name).SetProperty(p => p.Description, desc));
            }

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Convert.ToInt32(roomId), out Room room))
                return;

            data.Promotion.Name = name;
            data.Promotion.Description = desc;
            room.SendPacket(new RoomEventComposer(data, data.Promotion));
        }
    }
}