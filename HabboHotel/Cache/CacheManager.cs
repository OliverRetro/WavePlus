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

            // Return the cached entry only if it's complete. A partial entry (missing look/username,
            // e.g. cached before the avatar was available) falls through to a failsafe DB fetch below.
            if (TryGetUser(id, out user) && !string.IsNullOrEmpty(user.Look) && !string.IsNullOrEmpty(user.Username))
                return user;

            GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(id);
            if (client?.GetHabbo() != null) {
                user = new UserCache(id, client.GetHabbo().Username, client.GetHabbo().Motto, client.GetHabbo().Look);
                _usersCached[id] = user;
                return user;
            }

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var dRow = db.Users.Where(u => u.Id == id)
                    .Select(u => new { u.Username, u.Motto, u.Look })
                    .FirstOrDefault();

                if (dRow != null) {
                    user = new UserCache(id, dRow.Username, dRow.Motto, dRow.Look);
                    _usersCached[id] = user;
                }
            }

            return user;
        }

        public List<UserCache> GenerateUsers(IEnumerable<int> ids)
        {
            var result = new List<UserCache>();
            var missing = new List<int>();

            foreach (int id in ids.Distinct()) {
                if (TryGetUser(id, out UserCache cached) && !string.IsNullOrEmpty(cached.Look) && !string.IsNullOrEmpty(cached.Username)) {
                    result.Add(cached);
                    continue;
                }

                GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(id);
                if (client?.GetHabbo() != null) {
                    UserCache user = new UserCache(id, client.GetHabbo().Username, client.GetHabbo().Motto, client.GetHabbo().Look);
                    _usersCached[id] = user;
                    result.Add(user);
                    continue;
                }

                missing.Add(id);
            }

            if (missing.Count > 0) {
                using WavePlusContext db = PlusEnvironment.GetDbContext();
                var rows = db.Users.Where(u => missing.Contains(u.Id))
                    .Select(u => new { u.Id, u.Username, u.Motto, u.Look })
                    .ToList();

                foreach (var row in rows) {
                    UserCache user = new UserCache(row.Id, row.Username, row.Motto, row.Look);
                    _usersCached[row.Id] = user;
                    result.Add(user);
                }
            }

            return result;
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