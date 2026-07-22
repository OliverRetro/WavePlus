using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    internal class ApplyDecorationEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
                return;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out Room room))
                return;

            if (!room.CheckRights(session, true))
                return;

            Item item = session.GetHabbo().GetInventoryComponent().GetItem(packet.PopInt());

            if (item?.GetBaseItem() == null)
                return;

            string decorationKey = string.Empty;
            switch (item.GetBaseItem().InteractionType) {
                case InteractionType.Floor:
                    decorationKey = "floor";
                    break;

                case InteractionType.Wallpaper:
                    decorationKey = "wallpaper";
                    break;

                case InteractionType.Landscape:
                    decorationKey = "landscape";
                    break;
            }

            switch (decorationKey) {
                case "floor":
                    room.Floor = item.ExtraData;

                    PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.FurniDecoFloor);
                    PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_RoomDecoFloor", 1);
                    break;

                case "wallpaper":
                    room.Wallpaper = item.ExtraData;

                    PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.FurniDecoWall);
                    PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_RoomDecoWallpaper", 1);
                    break;

                case "landscape":
                    room.Landscape = item.ExtraData;

                    PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_RoomDecoLandscape", 1);
                    break;
            }

            int rid = room.RoomId;
            uint itemDbId = (uint)item.Id;
            string extraData = item.ExtraData;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                switch (decorationKey) {
                    case "floor":
                        db.Rooms.Where(r => r.Id == rid).ExecuteUpdate(s => s.SetProperty(r => r.Floor, extraData));
                        break;

                    case "wallpaper":
                        db.Rooms.Where(r => r.Id == rid).ExecuteUpdate(s => s.SetProperty(r => r.Wallpaper, extraData));
                        break;

                    case "landscape":
                        db.Rooms.Where(r => r.Id == rid).ExecuteUpdate(s => s.SetProperty(r => r.Landscape, extraData));
                        break;
                }

                db.Items.Where(i => i.Id == itemDbId).ExecuteDelete();
            }

            session.GetHabbo().GetInventoryComponent().RemoveItem(item.Id);
            room.SendPacket(new RoomPropertyComposer(decorationKey, item.ExtraData));
        }
    }
}