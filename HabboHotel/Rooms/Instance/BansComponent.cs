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
        private Room _instance;

        private ConcurrentDictionary<int, double> _bans;

        public BansComponent(Room instance)
        {
            if (instance == null)
                return;

            _instance = instance;
            _bans = new ConcurrentDictionary<int, double>();

            uint roomId = (uint)_instance.Id;
            long nowSec = (long)PlusEnvironment.GetUnixTimestamp();
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                foreach (var row in db.RoomBans.Where(b => b.RoomId == roomId && b.Expire > nowSec).Select(b => new { b.UserId, b.Expire }).ToList())
                    _bans.TryAdd((int)row.UserId, row.Expire);
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
                db.RoomBans.Upsert(new RoomBanEntity
                {
                    UserId = (uint)avatar.UserId,
                    RoomId = (uint)_instance.Id,
                    Expire = (int)banTime
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
                    db.RoomBans.Where(b => b.RoomId == roomId && b.UserId == uid).ExecuteDelete();

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
                    db.RoomBans.Where(b => b.RoomId == roomId && b.UserId == uid).ExecuteDelete();

                return true;
            }

            return false;
        }

        public List<int> BannedUsers()
        {
            List<int> bans = new();

            uint roomId = (uint)_instance.Id;
            long nowSec = (long)PlusEnvironment.GetUnixTimestamp();
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                foreach (int uid in db.RoomBans.Where(b => b.RoomId == roomId && b.Expire > nowSec).Select(b => (int)b.UserId).ToList()) {
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