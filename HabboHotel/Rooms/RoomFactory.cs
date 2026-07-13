using System;
using System.Collections.Generic;
using System.Linq;
using Plus.Database.EF;

namespace Plus.HabboHotel.Rooms
{
    public static class RoomFactory
    {
        public static List<RoomData> GetRoomsDataByOwnerSortByName(int ownerId)
        {
            List<RoomData> data = new();
            string ownerStr = ownerId.ToString();

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            string username = db.Users.Where(u => u.Id == ownerId).Select(u => u.Username).FirstOrDefault();
            List<Database.EF.Entities.Room> rooms = db.Rooms.Where(r => r.Owner == ownerStr).OrderBy(r => r.Caption).ToList();

            foreach (Database.EF.Entities.Room r in rooms)
            {
                if (PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(r.Id, out Room room))
                {
                    data.Add(room);
                }
                else
                {
                    if (!PlusEnvironment.GetGame().GetRoomManager().TryGetModel(r.ModelName, out RoomModel model))
                        continue;

                    data.Add(BuildRoomData(r, username, model));
                }
            }

            return data;
        }

        public static bool TryGetData(int roomId, out RoomData data)
        {
            if (PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(roomId, out Room room))
            {
                data = room;
                return true;
            }

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            Database.EF.Entities.Room r = db.Rooms.FirstOrDefault(x => x.Id == roomId);

            if (r != null)
            {
                if (!PlusEnvironment.GetGame().GetRoomManager().TryGetModel(r.ModelName, out RoomModel model))
                {
                    data = null;
                    return false;
                }

                int ownerId = Convert.ToInt32(r.Owner);
                string username = db.Users.Where(u => u.Id == ownerId).Select(u => u.Username).FirstOrDefault();
                username = !string.IsNullOrEmpty(username) ? username : "Habboon";

                data = BuildRoomData(r, username, model);
                return true;
            }

            data = null;
            return false;
        }

        /// <summary>
        /// Maps a scaffolded <c>rooms</c> row to a <see cref="RoomData"/>. Several tinyint(1)
        /// columns scaffold as nullable bool and a couple (spush_enabled/lay_enabled/safezone)
        /// as enum-strings; both are normalised back to the flags/ints RoomData expects.
        /// </summary>
        private static RoomData BuildRoomData(Database.EF.Entities.Room r, string username, RoomModel model)
        {
            return new RoomData(r.Id, r.Caption, r.ModelName, username, Convert.ToInt32(r.Owner),
                r.Password, r.Score, r.Roomtype, r.State, r.UsersNow,
                r.UsersMax, r.Category, r.Description, r.Tags, r.Floor,
                r.Landscape, r.AllowPets ? 1 : 0, r.AllowPetsEat ? 1 : 0, r.RoomBlockingDisabled ? 1 : 0, r.AllowHidewall == true ? 1 : 0,
                r.Wallthick, r.Floorthick, r.Wallpaper, r.MuteSettings == true ? 1 : 0, r.BanSettings == true ? 1 : 0,
                r.KickSettings, r.ChatMode, r.ChatSize, r.ChatSpeed, r.ChatExtraFlood,
                r.ChatHearingDistance, r.TradeSettings, r.PushEnabled == true, r.PullEnabled == true,
                r.SpushEnabled == "1", r.SpullEnabled == true, r.EnablesEnabled == true, r.RespectNotificationsEnabled == true,
                r.PetMorphsAllowed == true, (int)r.GroupId, r.SalePrice, r.LayEnabled == "1", model, r.Safezone == "1");
        }
    }
}
