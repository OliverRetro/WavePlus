using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Plus.Database.EF;
using Plus.HabboHotel.Cache.Process;
using Plus.HabboHotel.Cache.Type;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Cache
{
    public class CacheManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CacheManager));
        private readonly ConcurrentDictionary<int, UserCache> _usersCached;
        private readonly ProcessComponent _process;

        public CacheManager()
        {
            _usersCached = new ConcurrentDictionary<int, UserCache>();
            _process = new ProcessComponent();
            _process.Init();
            Log.Info("Cache Manager -> LOADED");
        }

        public bool ContainsUser(int id)
        {
            return _usersCached.ContainsKey(id);
        }

        public UserCache GenerateUser(int id)
        {
            UserCache user = null;

            if (_usersCached.ContainsKey(id))
                if (TryGetUser(id, out user))
                    return user;

            GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(id);
            if (client?.GetHabbo() != null) {
                user = new UserCache(id, client.GetHabbo().Username, client.GetHabbo().Motto, client.GetHabbo().Look);
                _usersCached.TryAdd(id, user);
                return user;
            }

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var dRow = db.users.Where(u => u.id == id)
                    .Select(u => new { u.username, u.motto, u.look })
                    .FirstOrDefault();

                if (dRow != null) {
                    user = new UserCache(id, dRow.username, dRow.motto, dRow.look);
                    _usersCached.TryAdd(id, user);
                }
            }

            return user;
        }

        public bool TryRemoveUser(int id, out UserCache user)
        {
            return _usersCached.TryRemove(id, out user);
        }

        public bool TryGetUser(int id, out UserCache user)
        {
            return _usersCached.TryGetValue(id, out user);
        }

        public ICollection<UserCache> GetUserCache()
        {
            return _usersCached.Values;
        }
    }
}