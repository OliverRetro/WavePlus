using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Furni;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    internal class UpdateMagicTileEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
                return;

            Room room = session.GetHabbo().CurrentRoom;
            if (room == null)
                return;

            if (!room.CheckRights(session, false, true) && !session.GetHabbo().GetPermissions().HasRight("room_item_use_any_stack_tile"))
                return;

            int itemId = packet.PopInt();
            int decimalHeight = packet.PopInt();

            Item item = room.GetRoomItemHandler().GetItem(itemId);
            if (item == null)
                return;

            item.GetZ = decimalHeight / 100.0;

            uint magicItemId = (uint)item.Id;
            double magicZ = item.GetZ;
            using (Database.EF.WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.Items.Where(i => i.Id == magicItemId).ExecuteUpdate(s => s.SetProperty(i => i.Z, magicZ));
            }

            room.SendPacket(new ObjectUpdateComposer(item, Convert.ToInt32(session.GetHabbo().Id)));
            room.SendPacket(new UpdateMagicTileComposer(itemId, decimalHeight));
        }
    }
}