using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    internal class SetTonerEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out Room room))
                return;

            if (!room.CheckRights(session, true))
                return;

            if (room.TonerData == null)
                return;

            Item item = room.GetRoomItemHandler().GetItem(room.TonerData.ItemId);

            if (item == null || item.GetBaseItem().InteractionType != InteractionType.Toner)
                return;

            packet.PopInt(); //id
            int int1 = packet.PopInt();
            int int2 = packet.PopInt();
            int int3 = packet.PopInt();

            if (int1 > 255 || int2 > 255 || int3 > 255)
                return;

            uint itemDbId = (uint)item.Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.RoomItemsToners.Where(t => t.Id == itemDbId).ExecuteUpdate(s => s
                    .SetProperty(t => t.Enabled, "1")
                    .SetProperty(t => t.Data1, int1)
                    .SetProperty(t => t.Data2, int2)
                    .SetProperty(t => t.Data3, int3));
            }

            room.TonerData.Hue = int1;
            room.TonerData.Saturation = int2;
            room.TonerData.Lightness = int3;
            room.TonerData.Enabled = 1;

            room.SendPacket(new ObjectUpdateComposer(item, room.OwnerId));
            item.UpdateState();
        }
    }
}