using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Plus.Core;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms
{
    public class RoomManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RoomManager));

        private readonly object _roomLoadingSync;

        private readonly Dictionary<string, RoomModel> _roomModels;

        private readonly ConcurrentDictionary<int, Room> _rooms;

        private DateTime _cycleLastExecution;

        public RoomManager()
        {
            _roomModels = new Dictionary<string, RoomModel>();
            _rooms = new ConcurrentDictionary<int, Room>();
            _roomLoadingSync = new object();
        }

        public void OnCycle()
        {
            try
            {
                TimeSpan sinceLastTime = DateTime.Now - _cycleLastExecution;
                if (sinceLastTime.TotalMilliseconds >= 500)
                {
                    _cycleLastExecution = DateTime.Now;
                    foreach (Room room in _rooms.Values)
                    {
                        if (room.IsCrashed)
                            continue;

                        if (room.ProcessTask == null || room.ProcessTask.IsCompleted)
                        {
                            room.ProcessTask = Task.Run(room.ProcessRoom);
                            room.IsLagging = 0;
                        }
                        else
                        {
                            room.IsLagging++;
                            if (room.IsLagging >= 30)
                            {
                                room.IsCrashed = true;
                                UnloadRoom(room.Id);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        public int Count => _rooms.Count;

        public void LoadModels()
        {
            if (_roomModels.Count > 0)
                _roomModels.Clear();

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            var models = db.RoomModels.Where(m => m.Custom == "0")
                .Select(m => new { m.Id, m.DoorX, m.DoorY, m.DoorZ, m.DoorDir, m.Heightmap, m.ClubOnly, m.WallHeight })
                .ToList();

            foreach (var m in models)
            {
                if (m.Id != null)
                    _roomModels.Add(m.Id, new RoomModel(m.Id, m.DoorX, m.DoorY, m.DoorZ, m.DoorDir,
                        m.Heightmap, PlusEnvironment.EnumToBool(m.ClubOnly), m.WallHeight, false));
            }
        }

        public bool LoadModel(string id)
        {
            using WavePlusContext db = PlusEnvironment.GetDbContext();
            var m = db.RoomModels.Where(x => x.Custom == "1" && x.Id == id)
                .Select(x => new { x.Id, x.DoorX, x.DoorY, x.DoorZ, x.DoorDir, x.Heightmap, x.ClubOnly, x.WallHeight })
                .FirstOrDefault();

            if (m == null)
                return false;

            if (m.Id != null && !_roomModels.ContainsKey(m.Id))
            {
                _roomModels.Add(m.Id, new RoomModel(m.Id, m.DoorX, m.DoorY, m.DoorZ, m.DoorDir,
                    m.Heightmap, PlusEnvironment.EnumToBool(m.ClubOnly), m.WallHeight, true));
            }

            return true;
        }

        public void ReloadModel(string id)
        {
            if (!_roomModels.ContainsKey(id))
            {
                LoadModel(id);
                return;
            }

            _roomModels.Remove(id);
            LoadModel(id);
        }

        public bool TryGetModel(string id, out RoomModel model)
        {
            if (_roomModels.ContainsKey(id))
            {
                model = _roomModels[id];
                return true;
            }

            // Try to load this model.
            if (LoadModel(id))
            {
                if (TryGetModel(id, out RoomModel customModel))
                {
                    model = customModel;
                    return true;
                }
            }

            model = null;
            return false;
        }

        public void UnloadRoom(int roomId)
        {
            if (_rooms.TryRemove(roomId, out Room room))
            {
                room.Dispose();
            }
        }

        public bool TryLoadRoom(int roomId, out Room room)
        {
            if (_rooms.TryGetValue(roomId, out Room inst))
            {
                if (!inst.Unloaded)
                {
                    room = inst;
                    return true;
                }

                room = null;
                return false;
            }

            lock (_roomLoadingSync)
            {
                if (_rooms.TryGetValue(roomId, out inst))
                {
                    if (!inst.Unloaded)
                    {
                        room = inst;
                        return true;
                    }

                    room = null;
                    return false;
                }

                if (!RoomFactory.TryGetData(roomId, out RoomData data))
                {
                    room = null;
                    return false;
                }

                Room myInstance = new(data);
                if (_rooms.TryAdd(roomId, myInstance))
                {
                    room = myInstance;
                    return true;
                }

                room = null;
                return false;
            }
        }

        public List<Room> SearchGroupRooms(string query)
        {
            return _rooms.Values.Where(x => x.Group != null && x.Group.Name.ToLower().Contains(query.ToLower()) && x.Access != RoomAccess.Invisible).OrderByDescending(x => x.UsersNow).Take(50).ToList();
        }

        public List<Room> SearchTaggedRooms(string query)
        {
            return _rooms.Values.Where(x => x.Tags.Contains(query) && x.Access != RoomAccess.Invisible).OrderByDescending(x => x.UsersNow).Take(50).ToList();
        }

        public List<Room> GetPopularRooms(int category, int amount = 50)
        {
            return _rooms.Values.Where(x => x.UsersNow > 0 && x.Access != RoomAccess.Invisible).OrderByDescending(x => x.UsersNow).Take(amount).ToList();
        }

        public List<Room> GetRecommendedRooms(int amount = 50, int currentRoomId = 0)
        {
            return _rooms.Values.Where(x => x.Id != currentRoomId && x.Access != RoomAccess.Invisible).OrderByDescending(x => x.UsersNow).ThenByDescending(x => x.Score).Take(amount).ToList();
        }

        public List<Room> GetPopularRatedRooms(int amount = 50)
        {
            return _rooms.Values.Where(x => x.Access != RoomAccess.Invisible).OrderByDescending(x => x.Score).ThenByDescending(x => x.UsersNow).Take(amount).ToList();
        }

        public List<Room> GetRoomsByCategory(int category, int amount = 50)
        {
            return _rooms.Values.Where(x => x.Category == category && x.Access != RoomAccess.Invisible && x.UsersNow > 0).OrderByDescending(x => x.UsersNow).Take(amount).ToList();
        }

        public List<Room> GetOnGoingRoomPromotions(int mode, int amount = 50)
        {
            if (mode == 17)
            {
                return _rooms.Values.Where(x => x.HasActivePromotion && x.Access != RoomAccess.Invisible).OrderByDescending(x => x.Promotion.TimestampStarted).Take(amount).ToList();
            }

            return _rooms.Values.Where(x => x.HasActivePromotion && x.Access != RoomAccess.Invisible).OrderByDescending(x => x.UsersNow).Take(amount).ToList();
        }

        public List<Room> GetPromotedRooms(int categoryId, int amount = 50)
        {
            return _rooms.Values.Where(x => x.HasActivePromotion && x.Promotion.CategoryId == categoryId && x.Access != RoomAccess.Invisible).OrderByDescending(x => x.Promotion.TimestampStarted).Take(amount).ToList();
        }

        public List<Room> GetGroupRooms(int amount = 50)
        {
            return _rooms.Values.Where(x => x.Group != null && x.Access != RoomAccess.Invisible).OrderByDescending(x => x.Score).Take(amount).ToList();
        }

        public List<Room> GetRoomsByIds(List<int> ids, int amount = 50)
        {
            return _rooms.Values.Where(x => ids.Contains(x.Id) && x.Access != RoomAccess.Invisible).OrderByDescending(x => x.UsersNow).Take(amount).ToList();
        }

        public Room TryGetRandomLoadedRoom()
        {
            return _rooms.Values.Where(x => x.UsersNow > 0 && x.Access != RoomAccess.Invisible && x.UsersNow < x.UsersMax).OrderByDescending(x => x.UsersNow).FirstOrDefault();
        }

        public bool TryGetRoom(int roomId, out Room room)
        {
            return _rooms.TryGetValue(roomId, out room);
        }

        public RoomData CreateRoom(GameClient session, string name, string description, int category, int maxVisitors, int tradeSettings, RoomModel model, string wallpaper = "0.0", string floor = "0.0", string landscape = "0.0", int wallThick = 0, int floorThick = 0)
        {
            if (name.Length < 3)
            {
                session.SendNotification(PlusEnvironment.GetLanguageManager().TryGetValue("room.creation.name.too_short"));
                return null;
            }

            int roomId;

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var newRoom = new Database.EF.Entities.Room
                {
                    Roomtype = "private",
                    Caption = name,
                    Description = description,
                    Owner = session.GetHabbo().Id.ToString(),
                    ModelName = model.Id,
                    Category = category,
                    UsersMax = maxVisitors,
                    TradeSettings = tradeSettings
                };
                db.Rooms.Add(newRoom);
                db.SaveChanges();
                roomId = newRoom.Id;
            }

            RoomData data = new(roomId, name, model.Id, session.GetHabbo().Username, session.GetHabbo().Id, "", 0, "public", "open", 0, maxVisitors, category, description, string.Empty,
                floor, landscape, 1, 1, 0, 0, wallThick, floorThick, wallpaper, 1, 1, 1, 1, 1, 1, 1, 8, tradeSettings, true, true, true, true, true, true, true, 0, 0, true, model, false);

            return data;
        }

        public ICollection<Room> GetRooms()
        {
            return _rooms.Values;
        }

        public IEnumerable<RoomUser> GetAllRoomUsers()
        {
            foreach (Room room in _rooms.Values)
            {
                if (room == null)
                    continue;

                RoomUserManager userManager = room.GetRoomUserManager();
                if (userManager == null)
                    continue;

                foreach (RoomUser user in userManager.GetUserList())
                {
                    if (user != null)
                        yield return user;
                }
            }
        }

        public void Dispose()
        {
            int length = _rooms.Count;
            int i = 0;
            foreach (Room room in _rooms.Values)
            {
                if (room == null)
                    continue;

                PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(room.Id);
                Console.Clear();
                Log.Info("<<- SERVER SHUTDOWN ->> ROOM ITEM SAVE: " + string.Format("{0:0.##}", ((double)i / length) * 100) + "%");
                i++;
            }

            Log.Info("Done disposing rooms!");
        }
    }
}