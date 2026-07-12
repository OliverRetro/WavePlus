using System.Collections.Generic;
using System.Linq;
using Plus.Database.EF;

namespace Plus.HabboHotel.Users.Ignores
{
    public sealed class IgnoresComponent
    {
        private readonly List<int> _ignoredUsers;

        public IgnoresComponent()
        {
            _ignoredUsers = new List<int>();
        }

        public bool Init(Habbo player)
        {
            if (_ignoredUsers.Count > 0)
                return false;

            uint uid = (uint)player.Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                _ignoredUsers.AddRange(db.user_ignores.Where(x => x.user_id == uid).Select(x => (int)x.ignore_id).ToList());

            return true;
        }

        public bool TryGet(int userId)
        {
            return _ignoredUsers.Contains(userId);
        }

        public bool TryAdd(int userId)
        {
            if (_ignoredUsers.Contains(userId))
                return false;

            _ignoredUsers.Add(userId);
            return true;
        }

        public bool TryRemove(int userId)
        {
            return _ignoredUsers.Remove(userId);
        }

        public ICollection<int> IgnoredUserIds()
        {
            return _ignoredUsers;
        }

        public void Dispose()
        {
            _ignoredUsers.Clear();
        }
    }
}