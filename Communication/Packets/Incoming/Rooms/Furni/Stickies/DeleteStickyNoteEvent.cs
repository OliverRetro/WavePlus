using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni.Stickies
{
    internal class DeleteStickyNoteEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
                return;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out Room room))
                return;

            if (!room.CheckRights(session))
                return;

            Item item = room.GetRoomItemHandler().GetItem(packet.PopInt());
            if (item == null)
                return;

            if (item.GetBaseItem().InteractionType == InteractionType.PostIt || item.GetBaseItem().InteractionType == InteractionType.CameraPicture) {
                room.GetRoomItemHandler().RemoveFurniture(session, item.Id);
                uint itemDbId = (uint)item.Id;
                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    db.Items.Where(i => i.Id == itemDbId).ExecuteDelete();
                }
            }
        }
    }
}