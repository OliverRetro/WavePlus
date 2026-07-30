using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Plus.Database.EF;
using Plus.HabboHotel.Rooms;

namespace Plus.Core.Cache
{
    // Cache every room configured as public.
    public static class PublicRoomCache
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PublicRoomCache));

        public const string IndexKey = "rooms:public:ids";

        private static string RoomKey(int roomId) => $"room:public:{roomId}";

        public static void LoadAll()
        {
            List<int> ids;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                ids = [.. db.Rooms.Where(r => r.Roomtype == "public").Select(r => r.Id)];

            int loaded = 0;
            foreach (int id in ids) {
                try {
                    if (PlusEnvironment.GetGame().GetRoomManager().TryLoadRoom(id, out Room room)) {
                        Mirror(room);
                        loaded++;
                    }
                } catch (Exception ex) {
                    Log.Error($"Failed to preload public room {id}: {ex.Message}");
                }
            }

            Log.Info($"Preloaded {loaded} public room(s) into memory (mirrored to Redis).");
        }

        // Mirrors every loaded public room. Run by the 60-second Redis mirror.
        public static void MirrorAll()
        {
            foreach (Room room in PlusEnvironment.GetGame().GetRoomManager().GetRooms()) {
                if (room != null && room.IsPublic)
                    Mirror(room);
            }
        }

        public static void Mirror(Room room)
        {
            RedisManager redis = PlusEnvironment.GetRedis();
            if (redis == null || !redis.IsAvailable || room == null)
                return;

            var snapshot = new
            {
                room.Id,
                room.Name,
                room.ModelName,
                room.OwnerId,
                room.OwnerName,
                room.Description,
                room.UsersNow,
                room.UsersMax,
                room.Wallpaper,
                room.Floor,
                room.Landscape,
                Items = room.GetRoomItemHandler().GetWallAndFloor.Select(i => new
                {
                    i.Id,
                    i.BaseItem,
                    Name = i.GetBaseItem()?.ItemName,
                    X = i.GetX,
                    Y = i.GetY,
                    Z = i.GetZ,
                    Rot = i.Rotation,
                    i.WallCoord,
                    i.ExtraData,
                    Wall = i.IsWallItem
                }).ToList()
            };

            redis.SetObject(RoomKey(room.Id), snapshot);
            redis.SetAdd(IndexKey, room.Id.ToString());
        }
    }
}