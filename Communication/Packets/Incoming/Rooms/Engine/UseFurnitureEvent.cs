using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Furni;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.Wired;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    internal class UseFurnitureEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().InRoom)
                return;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out Room room))
                return;

            int itemId = packet.PopInt();
            Item item = room.GetRoomItemHandler().GetItem(itemId);
            if (item == null)
                return;

            bool hasRights = room.CheckRights(session, false, true);

            if (item.GetBaseItem().InteractionType == InteractionType.BanzaiTele)
                return;

            if (item.GetBaseItem().InteractionType == InteractionType.Toner) {
                if (!room.CheckRights(session, true))
                    return;

                room.TonerData.Enabled = room.TonerData.Enabled == 0 ? 1 : 0;

                room.SendPacket(new ObjectUpdateComposer(item, room.OwnerId));

                item.UpdateState();

                // NOTE: preserves legacy behavior - no WHERE clause, updates a single (first) row via LIMIT 1.
                string tonerEnabled = room.TonerData.Enabled.ToString();
                using (WavePlusContext db = PlusEnvironment.GetDbContext())
                {
                    db.Database.ExecuteSqlInterpolated($"UPDATE `room_items_toner` SET `enabled` = {tonerEnabled} LIMIT 1");
                }

                return;
            }

            if (item.Data.InteractionType == InteractionType.GnomeBox && item.UserId == session.GetHabbo().Id) {
                session.SendPacket(new GnomeBoxComposer(item.Id));
            }

            bool toggle = true;
            if (item.GetBaseItem().InteractionType == InteractionType.WfFloorSwitch1 || item.GetBaseItem().InteractionType == InteractionType.WfFloorSwitch2) {
                RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
                if (user == null)
                    return;

                if (!Gamemap.TilesTouching(item.GetX, item.GetY, user.X, user.Y)) {
                    toggle = false;
                }
            }

            int request = packet.PopInt();

            item.Interactor.OnTrigger(session, item, request, hasRights);

            if (toggle)
                item.GetRoom().GetWired().TriggerEvent(WiredBoxType.TriggerStateChanges, session.GetHabbo(), item);

            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.ExploreFindItem, item.GetBaseItem().Id);
        }
    }
}