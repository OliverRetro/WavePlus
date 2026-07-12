using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
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

            uint roomId = (uint)_instance.Id;
            long nowSec = (long)PlusEnvironment.GetUnixTimestamp();
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                foreach (var row in db.room_bans.Where(b => b.room_id == roomId && b.expire > nowSec).Select(b => new { b.user_id, b.expire }).ToList())
                    _bans.TryAdd((int)row.user_id, row.expire);
            }
        }

        public void Ban(RoomUser avatar, double time)
        {
            if (avatar == null || _instance.CheckRights(avatar.GetClient(), true) || IsBanned(avatar.UserId))
                return;

            double banTime = UnixTimestamp.GetNow() + time;
            if (!_bans.TryAdd(avatar.UserId, banTime))
                _bans[avatar.UserId] = banTime;

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                db.room_bans.Upsert(new room_ban
                {
                    user_id = (uint)avatar.UserId,
                    room_id = (uint)_instance.Id,
                    expire = (int)banTime
                }).Run();

            _instance.GetRoomUserManager().RemoveUserFromRoom(avatar.GetClient(), true, true);
        }

        public bool IsBanned(int userId)
        {
            if (!_bans.ContainsKey(userId))
                return false;

            double banTime = _bans[userId] - UnixTimestamp.GetNow();
            if (banTime <= 0) {
                _bans.TryRemove(userId, out double time);

                uint roomId = (uint)_instance.Id;
                uint uid = (uint)userId;
                using (WavePlusContext db = PlusEnvironment.GetDbContext())
                    db.room_bans.Where(b => b.room_id == roomId && b.user_id == uid).ExecuteDelete();

                return false;
            }

            return true;
        }

        public bool Unban(int userId)
        {
            if (!_bans.ContainsKey(userId))
                return false;

            if (_bans.TryRemove(userId, out double time)) {
                uint roomId = (uint)_instance.Id;
                uint uid = (uint)userId;
                using (WavePlusContext db = PlusEnvironment.GetDbContext())
                    db.room_bans.Where(b => b.room_id == roomId && b.user_id == uid).ExecuteDelete();

                return true;
            }

            return false;
        }

        public List<int> BannedUsers()
        {
            List<int> bans = new();

            uint roomId = (uint)_instance.Id;
            long nowSec = (long)PlusEnvironment.GetUnixTimestamp();
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                foreach (int uid in db.room_bans.Where(b => b.room_id == roomId && b.expire > nowSec).Select(b => (int)b.user_id).ToList())
                {
                    if (!bans.Contains(uid))
                        bans.Add(uid);
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