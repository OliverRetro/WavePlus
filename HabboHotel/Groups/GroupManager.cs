using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using log4net;
using Plus.Database.EF;
using Microsoft.EntityFrameworkCore;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Groups
{
    public class GroupManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GroupManager));

        private readonly object _groupLoadingSync;
        private readonly ConcurrentDictionary<int, Group> _groups;
        private const int RoleSaveIntervalTicks = 24000;
        private int _roleSaveTick;
        private readonly ConcurrentDictionary<int, GroupPermissionKey> _permissionKeys;
        private readonly ConcurrentDictionary<string, GroupPermissionKey> _permissionKeysByKey;

        private readonly List<GroupBadgeParts> _bases;
        private readonly List<GroupBadgeParts> _symbols;
        private readonly List<GroupColours> _baseColours;
        private readonly Dictionary<int, GroupColours> _symbolColours;
        private readonly Dictionary<int, GroupColours> _backgroundColours;

        public GroupManager()
        {
            _groupLoadingSync = new object();
            _groups = new ConcurrentDictionary<int, Group>();
            _roleSaveTick = RoleSaveIntervalTicks;
            _permissionKeys = new ConcurrentDictionary<int, GroupPermissionKey>();
            _permissionKeysByKey = new ConcurrentDictionary<string, GroupPermissionKey>(StringComparer.OrdinalIgnoreCase);

            _bases = new List<GroupBadgeParts>();
            _symbols = new List<GroupBadgeParts>();
            _baseColours = new List<GroupColours>();
            _symbolColours = new Dictionary<int, GroupColours>();
            _backgroundColours = new Dictionary<int, GroupColours>();
        }

        public void Init()
        {
            _bases.Clear();
            _symbols.Clear();
            _baseColours.Clear();
            _symbolColours.Clear();
            _backgroundColours.Clear();
            _permissionKeys.Clear();
            _permissionKeysByKey.Clear();

            using WavePlusContext db = PlusEnvironment.GetDbContext();

            var groupItems = db.groups_items.Where(g => g.enabled == "1")
                .Select(g => new { g.id, g.type, g.firstvalue, g.secondvalue }).ToList();

            foreach (var groupItem in groupItems)
            {
                switch (groupItem.type)
                {
                    case "base":
                        _bases.Add(new GroupBadgeParts(groupItem.id, groupItem.firstvalue, groupItem.secondvalue));
                        break;

                    case "symbol":
                        _symbols.Add(new GroupBadgeParts(groupItem.id, groupItem.firstvalue, groupItem.secondvalue));
                        break;

                    case "color":
                        _baseColours.Add(new GroupColours(groupItem.id, groupItem.firstvalue));
                        break;

                    case "color2":
                        _symbolColours.Add(groupItem.id, new GroupColours(groupItem.id, groupItem.firstvalue));
                        break;

                    case "color3":
                        _backgroundColours.Add(groupItem.id, new GroupColours(groupItem.id, groupItem.firstvalue));
                        break;
                }
            }

            var permissionKeys = db.group_permission_keys
                .Select(k => new { k.id, k.name, k.key, k.description, k.type_specific }).ToList();
            foreach (var row in permissionKeys)
            {
                GroupPermissionKey permissionKey = new GroupPermissionKey(row.id, row.name, row.key, row.description, row.type_specific);

                _permissionKeys[permissionKey.Id] = permissionKey;
                _permissionKeysByKey[permissionKey.Key] = permissionKey;
            }
        }

        public void ReloadAll()
        {
            Init();

            foreach (Group group in _groups.Values)
            {
                group.InitRoles();
                group.InitPermissions();
            }
        }

        public bool TryGetGroup(int id, out Group group)
        {
            group = null;

            if (_groups.ContainsKey(id))
                return _groups.TryGetValue(id, out group);

            lock (_groupLoadingSync)
            {
                if (_groups.ContainsKey(id))
                    return _groups.TryGetValue(id, out group);

                uint gid = (uint)id;
                using WavePlusContext db = PlusEnvironment.GetDbContext();
                var r = db.groups.FirstOrDefault(x => x.id == gid);

                if (r != null)
                {
                    group = new Group((int)r.id, r.name, r.desc, r.badge, (int)r.room_id, (int)r.owner_id, r.created, Convert.ToInt32(r.state), r.group_type, r.colour1, r.colour2, Convert.ToInt32(r.admindeco), Convert.ToInt32(r.forum_enabled) == 1);
                    _groups.TryAdd(group.Id, group);
                    return true;
                }
            }

            return false;
        }

        public bool TryCreateGroup(Habbo player, string name, string description, int roomId, string badge, int colour1, int colour2, out Group group)
        {
            group = new Group(0, name, description, badge, roomId, player.Id, (int) PlusEnvironment.GetUnixTimestamp(), 0, 0, colour1, colour2, 0, false);
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(badge))
                return false;

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            var entity = new Database.EF.Entities.group
            {
                name = group.Name,
                desc = group.Description,
                badge = group.Badge,
                owner_id = (uint)group.CreatorId,
                created = (int)PlusEnvironment.GetUnixTimestamp(),
                room_id = (uint)group.RoomId,
                state = "0",
                group_type = 0,
                colour1 = group.Colour1,
                colour2 = group.Colour2,
                admindeco = "0"
            };
            db.groups.Add(entity);
            db.SaveChanges();
            group.Id = (int)entity.id;

            group.AddMember(player.Id, 0);
            group.AddRole("Member", 0, string.Empty, string.Empty, 5, true);

            if (!_groups.TryAdd(group.Id, group))
                return false;

            uint groupIdValue = (uint)group.Id;
            int roomIdInt = group.RoomId;
            uint roomIdValue = (uint)group.RoomId;
            db.rooms.Where(x => x.id == roomIdInt).ExecuteUpdate(s => s.SetProperty(x => x.group_id, groupIdValue));
            db.room_rights.Where(x => x.room_id == roomIdValue).ExecuteDelete();

            return true;
        }

        public void OnCycle()
        {
            _roleSaveTick--;
            if (_roleSaveTick > 0)
                return;

            SaveAll();
            _roleSaveTick = RoleSaveIntervalTicks;
        }

        public void SaveAll()
        {
            foreach (Group group in _groups.Values)
                group.Flush();
        }

        /// <summary>Every currently-loaded group (used by the Redis group mirror).</summary>
        public IEnumerable<Group> GetLoadedGroups() => _groups.Values;

        public bool TryGetPermissionKey(int permissionId, out GroupPermissionKey permissionKey)
        {
            return _permissionKeys.TryGetValue(permissionId, out permissionKey);
        }

        public bool TryGetPermissionKey(string permissionKeyName, out GroupPermissionKey permissionKey)
        {
            permissionKey = null;
            return !string.IsNullOrWhiteSpace(permissionKeyName) && _permissionKeysByKey.TryGetValue(permissionKeyName, out permissionKey);
        }

        public ICollection<GroupPermissionKey> PermissionKeys => _permissionKeys.Values.ToList();

        public string GetColourCode(int id, bool colourOne)
        {
            if (colourOne) {
                if (_symbolColours.ContainsKey(id)) {
                    return _symbolColours[id].Colour;
                }
            } else {
                if (_backgroundColours.ContainsKey(id)) {
                    return _backgroundColours[id].Colour;
                }
            }

            return "";
        }

        public void DeleteGroup(int id)
        {
            Group group = null;
            if (_groups.ContainsKey(id))
                _groups.TryRemove(id, out group);

            Plus.Core.Cache.GroupCache.Remove(id);
            group?.Dispose();
        }

        public static bool IsWorkableKind(GroupKind kind) => kind == GroupKind.Corporation || kind == GroupKind.Business;
        public bool TryGetWorkGroupForUser(int userId, out Group workGroup)
        {
            workGroup = null;
            foreach (Group group in GetGroupsForUser(userId))
            {
                if (group != null && IsWorkableKind(group.Kind)) {
                    workGroup = group;
                    return true;
                }
            }

            return false;
        }

        public List<Group> GetGroupsForUser(int userId)
        {
            List<Group> groups = new();
            HashSet<int> seen = new();

            // Loaded groups are the authoritative (cached) source: membership changes are deferred
            // to the 15-minute DB flush, so the DB alone would be stale for recent joins/leaves.
            foreach (Group group in _groups.Values)
            {
                if (group.IsMember(userId) && seen.Add(group.Id))
                    groups.Add(group);
            }

            // Fall back to the DB only for groups not currently loaded (last-flushed membership).
            uint uid = (uint)userId;
            List<int> ids;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                ids = db.group_memberships.Where(m => m.user_id == uid).Select(m => (int)m.group_id).ToList();
            }

            foreach (int gid in ids)
            {
                if (_groups.ContainsKey(gid))
                    continue; // already handled authoritatively above

                if (TryGetGroup(gid, out Group group) && group.IsMember(userId) && seen.Add(gid))
                    groups.Add(group);
            }

            return groups;
        }

        public ICollection<GroupBadgeParts> BadgeBases => _bases;

        public ICollection<GroupBadgeParts> BadgeSymbols => _symbols;

        public ICollection<GroupColours> BadgeBaseColours => _baseColours;

        public ICollection<GroupColours> BadgeSymbolColours => _symbolColours.Values;

        public ICollection<GroupColours> BadgeBackColours => _backgroundColours.Values;
    }
}
