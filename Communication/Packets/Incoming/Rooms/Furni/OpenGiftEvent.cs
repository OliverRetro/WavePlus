using System;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Furni;
using Plus.Database.EF;
using Plus.HabboHotel.Cache.Type;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    internal class OpenGiftEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().InRoom)
                return;

            Room room = session.GetHabbo().CurrentRoom;
            if (room == null)
                return;

            int presentId = packet.PopInt();
            Item present = room.GetRoomItemHandler().GetItem(presentId);
            if (present == null)
                return;

            if (present.UserId != session.GetHabbo().Id)
                return;

            uint presentKey = (uint)present.Id;
            int baseId;
            string extraData;
            bool found;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var row = db.UserPresents.Where(p => p.ItemId == presentKey).Select(p => new { p.BaseId, p.ExtraData }).FirstOrDefault();
                found = row != null;
                baseId = found ? (int)row.BaseId : 0;
                extraData = row?.ExtraData;
            }

            if (!found) {
                session.SendNotification("Oops! Appears there was a bug with this gift.\nWe'll just get rid of it for you.");
                room.GetRoomItemHandler().RemoveFurniture(null, present.Id);
                DeletePresent(present.Id);
                session.GetHabbo().GetInventoryComponent().RemoveItem(present.Id);
                return;
            }

            if (!int.TryParse(present.ExtraData.Split(Convert.ToChar(5))[2], out int purchaserId)) {
                session.SendNotification("Oops! Appears there was a bug with this gift.\nWe'll just get rid of it for you.");
                room.GetRoomItemHandler().RemoveFurniture(null, present.Id);
                DeletePresent(present.Id);
                session.GetHabbo().GetInventoryComponent().RemoveItem(present.Id);
                return;
            }

            UserCache purchaser = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(purchaserId);
            if (purchaser == null) {
                session.SendNotification("Oops! Appears there was a bug with this gift.\nWe'll just get rid of it for you.");
                room.GetRoomItemHandler().RemoveFurniture(null, present.Id);
                DeletePresent(present.Id);
                session.GetHabbo().GetInventoryComponent().RemoveItem(present.Id);
                return;
            }

            if (!PlusEnvironment.GetGame().GetItemManager().GetItem(baseId, out ItemData baseItem)) {
                session.SendNotification("Oops, it appears that the item within the gift is no longer in the hotel!");
                room.GetRoomItemHandler().RemoveFurniture(null, present.Id);
                DeletePresent(present.Id);
                session.GetHabbo().GetInventoryComponent().RemoveItem(present.Id);
                return;
            }

            present.MagicRemove = true;
            room.SendPacket(new ObjectUpdateComposer(present, Convert.ToInt32(session.GetHabbo().Id)));

            Thread thread = new(() => FinishOpenGift(session, baseItem, present, room, baseId, extraData));
            thread.Start();
        }

        private static void DeletePresent(int itemId)
        {
            uint pid = (uint)itemId;
            using WavePlusContext db = PlusEnvironment.GetDbContext();
            db.Items.Where(i => i.Id == pid).ExecuteDelete();
            db.UserPresents.Where(p => p.ItemId == pid).ExecuteDelete();
        }

        private void FinishOpenGift(GameClient session, ItemData baseItem, Item present, Room room, int baseId, string extraData)
        {
            try {
                if (baseItem == null || present == null || room == null)
                    return;

                Thread.Sleep(1500);

                bool itemIsInRoom = true;

                room.GetRoomItemHandler().RemoveFurniture(session, present.Id);

                uint itemId = (uint)present.Id;
                uint baseItemVal = (uint)baseId;
                string edata = extraData;
                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    db.Items.Where(i => i.Id == itemId).ExecuteUpdate(s => s.SetProperty(i => i.BaseItem, baseItemVal).SetProperty(i => i.ExtraData, edata));
                    db.UserPresents.Where(p => p.ItemId == itemId).ExecuteDelete();
                }

                present.BaseItem = baseId;
                present.ResetBaseItem();
                present.ExtraData = !string.IsNullOrEmpty(extraData) ? extraData : "";

                if (present.Data.Type == 's') {
                    if (!room.GetRoomItemHandler().SetFloorItem(session, present, present.GetX, present.GetY, present.Rotation, true, false, true)) {
                        using (WavePlusContext db = PlusEnvironment.GetDbContext())
                            db.Items.Where(i => i.Id == itemId).ExecuteUpdate(s => s.SetProperty(i => i.RoomId, (uint)0));

                        itemIsInRoom = false;
                    }
                } else {
                    using (WavePlusContext db = PlusEnvironment.GetDbContext())
                        db.Items.Where(i => i.Id == itemId).ExecuteUpdate(s => s.SetProperty(i => i.RoomId, (uint)0));

                    itemIsInRoom = false;
                }

                session.SendPacket(new OpenGiftComposer(present.Data, present.ExtraData, present, itemIsInRoom));

                session.GetHabbo().GetInventoryComponent().UpdateItems(true);
            } catch {
                //ignored
            }
        }
    }
}