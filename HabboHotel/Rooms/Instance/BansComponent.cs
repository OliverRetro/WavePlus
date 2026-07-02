using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using Plus.Database.Interfaces;
using Plus.Utilities;

namespace Plus.HabboHotel.Rooms.Instance
{
    public class BansComponent
    {
        /// <summary>
        /// The RoomInstance that created this BanComponent.
        /// </summary>
        private Room _instance;

        /// <summary>
        /// The bans collection for storing them for this room.
        /// </summary>
        private ConcurrentDictionary<int, double> _bans;

        /// <summary>
        /// Create the BanComponent for the RoomInstance.
        /// </summary>
        /// <param name="instance">The instance that created this component.</param>
        public BansComponent(Room instance)
        {
            if (instance == null)
                return;

            _instance = instance;
            _bans = new ConcurrentDictionary<int, double>();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `user_id`, `expire` FROM `room_bans` WHERE `room_id` = " + _instance.Id + " AND `expire` > UNIX_TIMESTAMP();");
                DataTable getBans = dbClient.GetTable();

                if (getBans != null) {
                    foreach (DataRow row in getBans.Rows)
                    {
                        _bans.TryAdd(Convert.ToInt32(row["user_id"]), Convert.ToDouble(row["expire"]));
                    }
                }
            }
        }

        public void Ban(RoomUser avatar, double time)
        {
            if (avatar == null || _instance.CheckRights(avatar.GetClient(), true) || IsBanned(avatar.UserId))
                return;

            double banTime = UnixTimestamp.GetNow() + time;
            if (!_bans.TryAdd(avatar.UserId, banTime))
                _bans[avatar.UserId] = banTime;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("REPLACE INTO `room_bans` (`user_id`,`room_id`,`expire`) VALUES (@uid, @rid, @expire);");
                dbClient.AddParameter("rid", _instance.Id);
                dbClient.AddParameter("uid", avatar.UserId);
                dbClient.AddParameter("expire", banTime);
                dbClient.RunQuery();
            }

            _instance.GetRoomUserManager().RemoveUserFromRoom(avatar.GetClient(), true, true);
        }

        public bool IsBanned(int userId)
        {
            if (!_bans.ContainsKey(userId))
                return false;

            double banTime = _bans[userId] - UnixTimestamp.GetNow();
            if (banTime <= 0) {
                _bans.TryRemove(userId, out double time);

                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("DELETE FROM `room_bans` WHERE `room_id` = @rid AND `user_id` = @uid;");
                    dbClient.AddParameter("rid", _instance.Id);
                    dbClient.AddParameter("uid", userId);
                    dbClient.RunQuery();
                }

                return false;
            }

            return true;
        }

        public bool Unban(int userId)
        {
            if (!_bans.ContainsKey(userId))
                return false;

            if (_bans.TryRemove(userId, out double time)) {
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("DELETE FROM `room_bans` WHERE `room_id` = @rid AND `user_id` = @uid;");
                    dbClient.AddParameter("rid", _instance.Id);
                    dbClient.AddParameter("uid", userId);
                    dbClient.RunQuery();
                }

                return true;
            }

            return false;
        }

        public List<int> BannedUsers()
        {
            List<int> bans = new();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `user_id` FROM `room_bans` WHERE `room_id` = '" + _instance.Id + "' AND `expire` > UNIX_TIMESTAMP();");
                DataTable getBans = dbClient.GetTable();

                if (getBans != null) {
                    foreach (DataRow row in getBans.Rows)
                    {
                        if (!bans.Contains(Convert.ToInt32(row["user_id"])))
                            bans.Add(Convert.ToInt32(row["user_id"]));
                    }
                }
            }

            return bans;
        }

        public int Count => _bans.Count;

        public void Cleanup()
        {
            _bans.Clear();

            _instance = null;
            _bans = null;
        }
    }
}