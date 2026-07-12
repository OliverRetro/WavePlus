using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    internal class DeleteRoomEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null)
                return;

            int roomId = packet.PopInt();
            if (roomId == 0)
                return;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(roomId, out Room room))
                return;

            if (room.OwnerId != session.GetHabbo().Id && !session.GetHabbo().GetPermissions().HasRight("room_delete_any"))
                return;

            List<Item> itemsToRemove = new();
            foreach (Item item in room.GetRoomItemHandler().GetWallAndFloor.ToList())
            {
                if (item == null)
                    continue;

                if (item.GetBaseItem().InteractionType == InteractionType.Moodlight) {
                    uint moodItemId = (uint)item.Id;
                    using (WavePlusContext db = PlusEnvironment.GetDbContext())
                        db.room_items_moodlights.Where(m => m.item_id == moodItemId).ExecuteDelete();
                }

                itemsToRemove.Add(item);
            }

            foreach (Item item in itemsToRemove)
            {
                GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(item.UserId);
                if (targetClient != null && targetClient.GetHabbo() != null) //Again, do we have an active client?
                {
                    room.GetRoomItemHandler().RemoveFurniture(targetClient, item.Id);
                    targetClient.GetHabbo().GetInventoryComponent().AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true, item.LimitedNo, item.LimitedTot);
                    targetClient.GetHabbo().GetInventoryComponent().UpdateItems(false);
                }
                else //No, query time.
                {
                    room.GetRoomItemHandler().RemoveFurniture(null, item.Id);
                    uint moveItemId = (uint)item.Id;
                    using (WavePlusContext db = PlusEnvironment.GetDbContext())
                        db.items.Where(i => i.id == moveItemId).ExecuteUpdate(s => s.SetProperty(i => i.room_id, 0u));
                }
            }

            PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(room.Id);

            uint roomIdU = (uint)roomId;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                db.user_roomvisits.Where(v => v.room_id == roomIdU).ExecuteDelete();
                db.rooms.Where(r => r.id == roomId).ExecuteDelete();
                db.user_favorites.Where(f => f.room_id == roomIdU).ExecuteDelete();
                db.items.Where(i => i.room_id == roomIdU).ExecuteDelete();
                db.room_rights.Where(r => r.room_id == roomIdU).ExecuteDelete();
                db.users.Where(u => u.home_room == roomId).ExecuteUpdate(s => s.SetProperty(u => u.home_room, 0));
            }

            PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(room.Id);
        }
    }
}