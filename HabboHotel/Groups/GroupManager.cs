using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Plus.Database.EF;
using Microsoft.EntityFrameworkCore;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Groups
{
    public class GroupManager
    {
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

            var groupItems = db.GroupsItems.Where(g => g.Enabled == "1")
                .Select(g => new { g.Id, g.Type, g.Firstvalue, g.Secondvalue }).ToList();

            foreach (var groupItem in groupItems) {
                switch (groupItem.Type) {
                    case "base":
                        _bases.Add(new GroupBadgeParts(groupItem.Id, groupItem.Firstvalue, groupItem.Secondvalue));
                        break;

                    case "symbol":
                        _symbols.Add(new GroupBadgeParts(groupItem.Id, groupItem.Firstvalue, groupItem.Secondvalue));
                        break;

                    case "color":
                        _baseColours.Add(new GroupColours(groupItem.Id, groupItem.Firstvalue));
                        break;

                    case "color2":
                        _symbolColours.Add(groupItem.Id, new GroupColours(groupItem.Id, groupItem.Firstvalue));
                        break;

                    case "color3":
                        _backgroundColours.Add(groupItem.Id, new GroupColours(groupItem.Id, groupItem.Firstvalue));
                        break;
                }
            }

            var permissionKeys = db.GroupPermissionKeys
                .Select(k => new { k.Id, k.Name, k.Key, k.Description, k.TypeSpecific }).ToList();
            foreach (var row in permissionKeys) {
                GroupPermissionKey permissionKey = new GroupPermissionKey(row.Id, row.Name, row.Key, row.Description, row.TypeSpecific);

                _permissionKeys[permissionKey.Id] = permissionKey;
                _permissionKeysByKey[permissionKey.Key] = permissionKey;
            }
        }

        public void ReloadAll()
        {
            Init();

            foreach (Group group in _groups.Values) {
                group.InitRoles();
                group.InitPermissions();
            }
        }

        public bool TryGetGroup(int id, out Group group)
        {
            group = null;

            if (_groups.ContainsKey(id))
                return _groups.TryGetValue(id, out group);

            lock (_groupLoadingSync) {
                if (_groups.ContainsKey(id))
                    return _groups.TryGetValue(id, out group);

                uint gid = (uint)id;
                using WavePlusContext db = PlusEnvironment.GetDbContext();
                var r = db.Groups.FirstOrDefault(x => x.Id == gid);

                if (r != null) {
                    group = new Group((int)r.Id, r.Name, r.Desc, r.Badge, (int)r.RoomId, (int)r.OwnerId, r.Created, Convert.ToInt32(r.State), r.GroupType, r.Colour1, r.Colour2, Convert.ToInt32(r.Admindeco), Convert.ToInt32(r.ForumEnabled) == 1);
                    _groups.TryAdd(group.Id, group);
                    return true;
                }
            }

            return false;
        }

        public bool TryCreateGroup(Habbo player, string name, string description, int roomId, string badge, int colour1, int colour2, out Group group)
        {
            group = new Group(0, name, description, badge, roomId, player.Id, (int)PlusEnvironment.GetUnixTimestamp(), 0, 0, colour1, colour2, 0, false);
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(badge))
                return false;

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            var entity = new Database.EF.Entities.GroupEntity
            {
                Name = group.Name,
                Desc = group.Description,
                Badge = group.Badge,
                OwnerId = (uint)group.CreatorId,
                Created = (int)PlusEnvironment.GetUnixTimestamp(),
                RoomId = (uint)group.RoomId,
                State = "0",
                GroupType = 0,
                Colour1 = group.Colour1,
                Colour2 = group.Colour2,
                Admindeco = "0"
            };
            db.Groups.Add(entity);
            db.SaveChanges();
            group.Id = (int)entity.Id;

            group.AddMember(player.Id, 0);
            group.AddRole("Member", 0, string.Empty, string.Empty, 5, true);

            if (!_groups.TryAdd(group.Id, group))
                return false;

            uint groupIdValue = (uint)group.Id;
            int roomIdInt = group.RoomId;
            uint roomIdValue = (uint)group.RoomId;
            db.Rooms.Where(x => x.Id == roomIdInt).ExecuteUpdate(s => s.SetProperty(x => x.GroupId, groupIdValue));
            db.RoomRights.Where(x => x.RoomId == roomIdValue).ExecuteDelete();

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

        public static bool IsGangKind(GroupKind kind) => kind == GroupKind.Gang || kind == GroupKind.Cartel || kind == GroupKind.Mafia;

        public bool TryGetGangForUser(int userId, out Group gang)
        {
            gang = null;
            foreach (Group group in GetGroupsForUser(userId)) {
                if (group != null && IsGangKind(group.Kind)) {
                    gang = group;
                    return true;
                }
            }

            return false;
        }

        public bool TryCreateGang(Habbo player, string name, string description, GroupKind kind, int colour1, int colour2, out Group group)
        {
            group = null;
            if (player == null || string.IsNullOrWhiteSpace(name) || !IsGangKind(kind))
                return false;

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            var entity = new Database.EF.Entities.GroupEntity
            {
                Name = name,
                Desc = description ?? string.Empty,
                Badge = GangDefinition.DefaultBadge,
                OwnerId = (uint)player.Id,
                Created = (int)PlusEnvironment.GetUnixTimestamp(),
                RoomId = 0,
                State = "0",
                GroupType = (short)kind,
                Colour1 = colour1,
                Colour2 = colour2,
                Admindeco = "0",
                ForumEnabled = "0"
            };
            db.Groups.Add(entity);
            db.SaveChanges();

            group = new Group((int)entity.Id, name, description ?? string.Empty, GangDefinition.DefaultBadge, 0, player.Id, entity.Created, 0, (int)kind, colour1, colour2, 0, false);

            group.AddMember(player.Id, 0);            // owner (level 0)
            group.AddOwnerRole();                     // persisted, undeletable Owner role at level 0
            group.AddRole("Member", 0, string.Empty, string.Empty, 5, true); // base role, level 1
            group.MarkDirty();

            return _groups.TryAdd(group.Id, group);
        }

        public void DeleteGangPermanent(int id)
        {
            DeleteGroup(id);

            uint gid = (uint)id;
            using WavePlusContext db = PlusEnvironment.GetDbContext();
            db.GroupMemberships.Where(m => m.GroupId == gid).ExecuteDelete();
            db.GroupRequests.Where(r => r.GroupId == gid).ExecuteDelete();
            db.GroupRoles.Where(r => r.GroupId == id).ExecuteDelete();
            db.GroupPermissions.Where(p => p.GroupId == id).ExecuteDelete();
            db.UserStats.Where(u => u.Groupid == id).ExecuteUpdate(s => s.SetProperty(u => u.Groupid, 0));
            db.Groups.Where(g => g.Id == gid).ExecuteDelete();
        }
        public bool TryGetWorkGroupForUser(int userId, out Group workGroup)
        {
            workGroup = null;
            foreach (Group group in GetGroupsForUser(userId)) {
                if (group != null && IsWorkableKind(group.Kind)) {
                    workGroup = group;
                    return true;
                }
            }

            return false;
        }

        public List<Group> GetGroupsForUser(int userId)
        {
            List<Group> groups = [];
            HashSet<int> seen = [];

            // Loaded groups are the authoritative (cached) source: membership changes are deferred
            // to the 15-minute DB flush, so the DB alone would be stale for recent joins/leaves.
            foreach (Group group in _groups.Values) {
                if (group.IsMember(userId) && seen.Add(group.Id))
                    groups.Add(group);
            }

            // Fall back to the DB only for groups not currently loaded (last-flushed membership).
            uint uid = (uint)userId;
            List<int> ids;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                ids = db.GroupMemberships.Where(m => m.UserId == uid).Select(m => (int)m.GroupId).ToList();
            }

            foreach (int gid in ids) {
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