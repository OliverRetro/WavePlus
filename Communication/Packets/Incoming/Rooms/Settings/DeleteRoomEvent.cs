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
            foreach (Item item in room.GetRoomItemHandler().GetWallAndFloor.ToList()) {
                if (item == null)
                    continue;

                if (item.GetBaseItem().InteractionType == InteractionType.Moodlight) {
                    uint moodItemId = (uint)item.Id;
                    using (WavePlusContext db = PlusEnvironment.GetDbContext())
                        db.RoomItemsMoodlights.Where(m => m.ItemId == moodItemId).ExecuteDelete();
                }

                itemsToRemove.Add(item);
            }

            foreach (Item item in itemsToRemove) {
                GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(item.UserId);
                if (targetClient != null && targetClient.GetHabbo() != null) //Again, do we have an active client?
                {
                    room.GetRoomItemHandler().RemoveFurniture(targetClient, item.Id);
                    targetClient.GetHabbo().GetInventoryComponent().AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true, item.LimitedNo, item.LimitedTot);
                    targetClient.GetHabbo().GetInventoryComponent().UpdateItems(false);
                } else //No, query time.
                  {
                    room.GetRoomItemHandler().RemoveFurniture(null, item.Id);
                    uint moveItemId = (uint)item.Id;
                    using (WavePlusContext db = PlusEnvironment.GetDbContext())
                        db.Items.Where(i => i.Id == moveItemId).ExecuteUpdate(s => s.SetProperty(i => i.RoomId, 0u));
                }
            }

            PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(room.Id);

            uint roomIdU = (uint)roomId;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.UserRoomvisits.Where(v => v.RoomId == roomIdU).ExecuteDelete();
                db.Rooms.Where(r => r.Id == roomId).ExecuteDelete();
                db.UserFavorites.Where(f => f.RoomId == roomIdU).ExecuteDelete();
                db.Items.Where(i => i.RoomId == roomIdU).ExecuteDelete();
                db.RoomRights.Where(r => r.RoomId == roomIdU).ExecuteDelete();
                db.Users.Where(u => u.HomeRoom == roomId).ExecuteUpdate(s => s.SetProperty(u => u.HomeRoom, 0));
            }

            PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(room.Id);
        }
    }
}