using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Core.Cache;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.Core.Persistence
{
    // Fail safe database saving
    public static class PersistenceRegistry
    {
        public static void RegisterAll(PersistenceScheduler scheduler)
        {
            // 5 minute saver
            scheduler.RegisterUserSaver("rp-user-data", SaveOnlineUserData);

            // 15 minute saver
            scheduler.RegisterWorldSaver("groups", () => PlusEnvironment.GetGame().GetGroupManager().SaveAll());
            scheduler.RegisterWorldSaver("chatlogs", () => PlusEnvironment.GetGame().GetChatManager().GetLogs().FlushAndSave());
            scheduler.RegisterWorldSaver("logs", LogBuffer.Flush);
            scheduler.RegisterWorldSaver("room-items", SaveLoadedRoomItems);
            scheduler.RegisterWorldSaver("bots", SaveLoadedRoomBots);
            scheduler.RegisterWorldSaver("room-stock", () => PlusEnvironment.GetRoomStockManager().Save());
            // Included in the world savers so the shutdown FlushAll() drains it too.
            scheduler.RegisterWorldSaver("room-visits", RoomVisitBuffer.Flush);

            // 60 second saver
            scheduler.RegisterMirror("user-mirror", MirrorOnlineUsers);
            scheduler.RegisterMirror("public-rooms", PublicRoomCache.MirrorAll);
            scheduler.RegisterMirror("groups", GroupCache.MirrorAll);
            scheduler.RegisterMirror("room-visits", RoomVisitBuffer.Flush);
            scheduler.RegisterMirror("room-usercounts", PersistRoomUserCounts);
        }

        private static void SaveOnlineUserData()
        {
            foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients) {
                Habbo habbo = client?.GetHabbo();
                if (habbo == null)
                    continue;

                habbo.SaveAllRpData();
                RpStatsCache.MirrorAll(habbo);
            }
        }

        private static void MirrorOnlineUsers()
        {
            foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients) {
                Habbo habbo = client?.GetHabbo();
                if (habbo != null)
                    RpStatsCache.MirrorAll(habbo);
            }
        }

        private static void PersistRoomUserCounts()
        {
            List<Room> dirty = null;
            foreach (Room room in PlusEnvironment.GetGame().GetRoomManager().GetRooms()) {
                if (room != null && room.UsersNow != room.LastPersistedUsersNow)
                    (dirty ??= []).Add(room);
            }

            if (dirty == null)
                return;

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            foreach (Room room in dirty) {
                int roomId = room.Id;
                int usersNow = room.UsersNow;
                db.Rooms.Where(r => r.Id == roomId)
                    .ExecuteUpdate(s => s.SetProperty(r => r.UsersNow, usersNow));
                room.LastPersistedUsersNow = usersNow;
            }
        }

        private static void SaveLoadedRoomItems()
        {
            foreach (Room room in PlusEnvironment.GetGame().GetRoomManager().GetRooms()) {
                room?.GetRoomItemHandler()?.FlushMovedItems();
            }
        }

        private static void SaveLoadedRoomBots()
        {
            using WavePlusContext db = PlusEnvironment.GetDbContext();

            foreach (Room room in PlusEnvironment.GetGame().GetRoomManager().GetRooms()) {
                if (room?.GetRoomUserManager() == null)
                    continue;

                foreach (RoomUser bot in room.GetRoomUserManager().GetBots()) {
                    if (bot?.BotData == null || bot.BotData.Id <= 0)
                        continue;

                    uint botId = (uint)bot.BotData.Id;
                    int x = bot.X, y = bot.Y, rot = bot.RotBody;
                    db.Bots.Where(b => b.Id == botId).ExecuteUpdate(s => s
                        .SetProperty(b => b.X, x)
                        .SetProperty(b => b.Y, y)
                        .SetProperty(b => b.Rotation, rot));
                }
            }
        }
    }
}