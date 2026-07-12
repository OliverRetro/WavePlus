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
            string username = db.users.Where(u => u.id == ownerId).Select(u => u.username).FirstOrDefault();
            List<Database.EF.Entities.room> rooms = db.rooms.Where(r => r.owner == ownerStr).OrderBy(r => r.caption).ToList();

            foreach (Database.EF.Entities.room r in rooms)
            {
                if (PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(r.id, out Room room)) {
                    data.Add(room);
                } else {
                    if (!PlusEnvironment.GetGame().GetRoomManager().TryGetModel(r.model_name, out RoomModel model))
                        continue;

                    data.Add(BuildRoomData(r, username, model));
                }
            }

            return data;
        }

        public static bool TryGetData(int roomId, out RoomData data)
        {
            if (PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(roomId, out Room room)) {
                data = room;
                return true;
            }

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            Database.EF.Entities.room r = db.rooms.FirstOrDefault(x => x.id == roomId);

            if (r != null) {
                if (!PlusEnvironment.GetGame().GetRoomManager().TryGetModel(r.model_name, out RoomModel model)) {
                    data = null;
                    return false;
                }

                int ownerId = Convert.ToInt32(r.owner);
                string username = db.users.Where(u => u.id == ownerId).Select(u => u.username).FirstOrDefault();
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
        private static RoomData BuildRoomData(Database.EF.Entities.room r, string username, RoomModel model)
        {
            return new RoomData(r.id, r.caption, r.model_name, username, Convert.ToInt32(r.owner),
                r.password, r.score, r.roomtype, r.state, r.users_now,
                r.users_max, r.category, r.description, r.tags, r.floor,
                r.landscape, r.allow_pets ? 1 : 0, r.allow_pets_eat ? 1 : 0, r.room_blocking_disabled ? 1 : 0, r.allow_hidewall == true ? 1 : 0,
                r.wallthick, r.floorthick, r.wallpaper, r.mute_settings == true ? 1 : 0, r.ban_settings == true ? 1 : 0,
                r.kick_settings, r.chat_mode, r.chat_size, r.chat_speed, r.chat_extra_flood,
                r.chat_hearing_distance, r.trade_settings, r.push_enabled == true, r.pull_enabled == true,
                r.spush_enabled == "1", r.spull_enabled == true, r.enables_enabled == true, r.respect_notifications_enabled == true,
                r.pet_morphs_allowed == true, (int)r.group_id, r.sale_price, r.lay_enabled == "1", model, r.safezone == "1");
        }
    }
}
