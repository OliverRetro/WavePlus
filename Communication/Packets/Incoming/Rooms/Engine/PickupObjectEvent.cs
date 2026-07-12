using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    internal class PickupObjectEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
                return;

            Room room = session.GetHabbo().CurrentRoom;
            if (room == null)
                return;

            packet.PopInt(); //unknown
            int itemId = packet.PopInt();

            Item item = room.GetRoomItemHandler().GetItem(itemId);
            if (item == null)
                return;

            if (item.GetBaseItem().InteractionType == InteractionType.PostIt)
                return;

            bool itemRights = false;
            if (item.UserId == session.GetHabbo().Id || room.CheckRights(session, false))
                itemRights = true;
            else if (room.Group != null && room.CheckRights(session, false, true)) //Room has a group, this user has group rights.
                itemRights = true;
            else if (session.GetHabbo().GetPermissions().HasRight("room_item_take"))
                itemRights = true;

            if (itemRights) {
                if (item.GetBaseItem().InteractionType == InteractionType.Tent || item.GetBaseItem().InteractionType == InteractionType.TentSmall)
                    room.RemoveTent(item.Id);

                uint itemDbId = (uint)item.Id;
                if (item.GetBaseItem().InteractionType == InteractionType.Moodlight) {
                    using (WavePlusContext db = PlusEnvironment.GetDbContext())
                    {
                        db.room_items_moodlights.Where(m => m.item_id == itemDbId).ExecuteDelete();
                    }
                } else if (item.GetBaseItem().InteractionType == InteractionType.Toner) {
                    using (WavePlusContext db = PlusEnvironment.GetDbContext())
                    {
                        db.room_items_toners.Where(t => t.id == itemDbId).ExecuteDelete();
                    }
                }


                if (item.UserId == session.GetHabbo().Id) {
                    room.GetRoomItemHandler().RemoveFurniture(session, item.Id);
                    session.GetHabbo().GetInventoryComponent().AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true, item.LimitedNo, item.LimitedTot);
                    session.GetHabbo().GetInventoryComponent().UpdateItems(false);
                } else if (session.GetHabbo().GetPermissions().HasRight("room_item_take")) //Staff are taking this item
                {
                    room.GetRoomItemHandler().RemoveFurniture(session, item.Id);
                    session.GetHabbo().GetInventoryComponent().AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true, item.LimitedNo, item.LimitedTot);
                    session.GetHabbo().GetInventoryComponent().UpdateItems(false);
                }
                else //Item is being ejected.
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
                        uint ejectItemId = (uint)item.Id;
                        using (WavePlusContext db = PlusEnvironment.GetDbContext())
                        {
                            db.items.Where(i => i.id == ejectItemId).ExecuteUpdate(s => s.SetProperty(i => i.room_id, (uint)0));
                        }
                    }
                }

                PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.FurniPick);
            }
        }
    }
}