using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Plus.Database.EF;
using Microsoft.EntityFrameworkCore;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Groups
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AdminOnlyDeco { get; set; }
        public string Badge { get; set; }
        public int CreateTime { get; set; }
        public int CreatorId { get; set; }
        public string Description { get; set; }
        public int RoomId { get; set; }
        public int Colour1 { get; set; }
        public int Colour2 { get; set; }
        public bool ForumEnabled { get; set; }
        public GroupType Type { get; set; }
        public GroupKind Kind { get; set; }
        public bool HasForum;
        private readonly ConcurrentDictionary<int, int> _memberLevels;
        private readonly List<int> _requests;
        private readonly ConcurrentDictionary<int, GroupRole> _roles;
        private readonly List<int> _deletedRoleIds;
        private readonly ConcurrentDictionary<string, GroupPermissionAssignment> _permissions;
        private readonly List<int> _deletedPermissionIds;

        // Set when metadata (name/desc/badge/colours/state/etc.) or the member/request sets change.
        // Persisted (and cleared) only by Flush() on the 15-minute world-save timer — never per-mutation.
        private volatile bool _dirty;

        private RoomData _room;

        // Hardcoded role for the owner (always level 0). Never persisted to the DB.
        private GroupRole _ownerRole;

        public Group(int id, string name, string description, string badge, int roomId, int owner, int time, int type, int groupType, int colour1, int colour2, int adminOnlyDeco, bool hasForum)
        {
            Id = id;
            Name = name;
            Description = description;
            RoomId = roomId;
            Badge = badge;
            CreateTime = time;
            CreatorId = owner;
            Colour1 = colour1 == 0 ? 1 : colour1;
            Colour2 = colour2 == 0 ? 1 : colour2;
            HasForum = hasForum;
            Type = (GroupType)type;
            Kind = (GroupKind)groupType;

            AdminOnlyDeco = adminOnlyDeco;
            ForumEnabled = ForumEnabled;

            _memberLevels = new ConcurrentDictionary<int, int>();
            _requests = new List<int>();
            _roles = new ConcurrentDictionary<int, GroupRole>();
            _deletedRoleIds = new List<int>();
            _permissions = new ConcurrentDictionary<string, GroupPermissionAssignment>(StringComparer.OrdinalIgnoreCase);
            _deletedPermissionIds = new List<int>();

            if (Id > 0) {
                InitMembers();
                InitRoles();
                InitPermissions();
            }
        }

        public void InitMembers()
        {
            uint gid = (uint)Id;
            using WavePlusContext db = PlusEnvironment.GetDbContext();

            var members = db.GroupMemberships.Where(m => m.GroupId == gid)
                .Select(m => new { m.UserId, m.Level }).ToList();

            foreach (var row in members) {
                int userId = (int)row.UserId;
                int level = row.Level;
                if (userId == CreatorId) {
                    level = 0;
                    if (row.Level != 0)
                        MarkDirty(); // creator level correction; persisted on the next flush
                }

                _memberLevels[userId] = level;
            }

            if (!_memberLevels.ContainsKey(CreatorId)) {
                _memberLevels[CreatorId] = 0;
                MarkDirty();
            }

            var requests = db.GroupRequests.Where(r => r.GroupId == gid).Select(r => (int)r.UserId).ToList();
            foreach (int userId in requests) {
                if (_memberLevels.ContainsKey(userId))
                    MarkDirty(); // stale request (already a member) is dropped on the next resync
                else if (!_requests.Contains(userId))
                    _requests.Add(userId);
            }
        }

        public void InitRoles()
        {
            int gid = Id;
            using WavePlusContext db = PlusEnvironment.GetDbContext();

            var roles = db.GroupRoles.Where(r => r.GroupId == gid).OrderBy(r => r.Level)
                .Select(r => new { r.Id, r.GroupId, r.Level, r.Name, r.ShiftPay, r.ShiftCostume, r.ShiftDuration, r.ShiftMotto, r.CreatedAt }).ToList();

            foreach (var row in roles) {
                GroupRole role = new(row.Id, row.GroupId, row.Level, row.Name, row.ShiftPay, row.ShiftCostume, row.ShiftDuration == null ? 5 : (int)row.ShiftDuration, row.ShiftMotto, row.CreatedAt);

                _roles[role.Level] = role;
            }
        }

        public void InitPermissions()
        {
            int gid = Id;
            using WavePlusContext db = PlusEnvironment.GetDbContext();

            var permissions = db.GroupPermissions.Where(p => p.GroupId == gid)
                .Select(p => new { p.Id, p.GroupId, p.LevelId, p.PermissionId, p.CreatedAt }).ToList();

            foreach (var row in permissions) {
                GroupPermissionAssignment assignment = new(
                    row.Id, row.GroupId, row.LevelId, row.PermissionId, row.CreatedAt);

                _permissions[BuildPermissionKey(assignment.LevelId, assignment.PermissionId)] = assignment;
            }
        }

        public List<int> GetMembers => _memberLevels.Where(x => x.Value == 1 && !IsHiddenFromLists(x.Value)).Select(x => x.Key).ToList();

        public List<int> GetRequests => _requests.ToList();

        public List<int> GetAdministrators => _memberLevels.Where(x => (x.Key == CreatorId || x.Value > 1) && !IsHiddenFromLists(x.Value)).Select(x => x.Key).ToList();

        private bool IsHiddenFromLists(int level)
        {
            return Kind == GroupKind.Corporation && level == 0;
        }

        public ICollection<GroupRole> GetRoles => _roles.Values.OrderBy(x => x.Level).ToList();

        public ICollection<GroupPermissionAssignment> GetPermissions => _permissions.Values.ToList();

        public ICollection<GroupPermissionKey> GetActivePermissionsForUser(int userId)
        {
            if (userId != CreatorId && !IsMember(userId))
                return Array.Empty<GroupPermissionKey>();

            ICollection<GroupPermissionKey> permissionKeys = PlusEnvironment.GetGame().GetGroupManager().PermissionKeys;
            return permissionKeys
                .Where(permission => HasPermission(userId, permission.Id))
                .OrderBy(permission => permission.Name)
                .ToList();
        }

        public List<int> GetAllMembers
        {
            get
            {
                return _memberLevels.Where(x => !IsHiddenFromLists(x.Value)).Select(x => x.Key).ToList();
            }
        }

        public int MemberCount => _memberLevels.Count(x => !IsHiddenFromLists(x.Value));

        public int RequestCount => _requests.Count;

        public bool IsMember(int id)
        {
            return _memberLevels.ContainsKey(id);
        }

        public bool IsAdmin(int id)
        {
            return _memberLevels.TryGetValue(id, out int level) && level > 1;
        }

        public bool HasRequest(int id)
        {
            return _requests.Contains(id);
        }

        public bool HasPermission(int userId, string permissionKey)
        {
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetPermissionKey(permissionKey, out GroupPermissionKey permission))
                return false;

            return HasPermission(userId, permission.Id);
        }

        public bool IsOwnerOrHasPermission(int userId, string permissionKey)
        {
            return userId == CreatorId || HasPermission(userId, permissionKey);
        }

        public bool RoleHasPermission(int level, int permissionId)
        {
            return _permissions.ContainsKey(BuildPermissionKey(level, permissionId));
        }

        public bool HasPermission(int userId, int permissionId)
        {
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetPermissionKey(permissionId, out GroupPermissionKey permission))
                return false;

            if (!permission.SupportsGroupKind(Kind))
                return false;

            if (userId == CreatorId)
                return true;

            int level = GetMemberLevel(userId);
            if (level < 1)
                return false;

            return _permissions.ContainsKey(BuildPermissionKey(level, permissionId));
        }

        public bool SetPermission(int levelId, int permissionId, bool enabled)
        {
            if (levelId < 1 || !_roles.ContainsKey(levelId))
                return false;

            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetPermissionKey(permissionId, out GroupPermissionKey permission))
                return false;

            string assignmentKey = BuildPermissionKey(levelId, permissionId);
            if (!permission.SupportsGroupKind(Kind)) {
                if (!enabled)
                    return true;

                return false;
            }

            if (!enabled) {
                if (!_permissions.TryRemove(assignmentKey, out GroupPermissionAssignment removed))
                    return true;

                if (removed.Id > 0)
                    _deletedPermissionIds.Add(removed.Id);

                return true;
            }

            if (_permissions.ContainsKey(assignmentKey))
                return true;

            _permissions[assignmentKey] = new GroupPermissionAssignment(0, Id, levelId, permissionId, (int)PlusEnvironment.GetUnixTimestamp(), true);
            return true;
        }

        public int GetMemberLevel(int id)
        {
            return _memberLevels.TryGetValue(id, out int level) ? level : -1;
        }

        public int GetRank(int userId)
        {
            if (userId == CreatorId)
                return int.MaxValue;

            if (!_memberLevels.TryGetValue(userId, out int level))
                return -1;

            return level == 0 ? int.MaxValue : level;
        }

        public bool TargetOutranks(int actorId, int targetId)
        {
            return GetRank(targetId) > GetRank(actorId);
        }

        public string GetMemberRoleName(int id)
        {
            if (!_memberLevels.TryGetValue(id, out int level))
                return string.Empty;

            if (id == CreatorId || level == 0)
                return "Owner";

            return _roles.TryGetValue(level, out GroupRole role) ? role.Name : level.ToString();
        }

        public bool TryGetRoleData(int id, out GroupRole role)
        {
            role = null;

            if (!_memberLevels.TryGetValue(id, out int level))
                return false;

            if (id == CreatorId || level == 0) {
                role = GetOwnerRole();
                return true;
            }

            if (_roles.TryGetValue(level, out GroupRole foundRole)) {
                role = foundRole;
                return true;
            }

            return false;
        }

        private GroupRole GetOwnerRole()
        {
            if (_roles.TryGetValue(0, out GroupRole persisted))
                return persisted;

            return _ownerRole ??= new GroupRole(0, Id, 0, "Owner", 0, string.Empty, 5, string.Empty, (int)PlusEnvironment.GetUnixTimestamp());
        }

        public GroupRole AddOwnerRole()
        {
            if (_roles.TryGetValue(0, out GroupRole existing))
                return existing;

            GroupRole role = new(0, Id, 0, "Owner", 0, string.Empty, 5, string.Empty, (int)PlusEnvironment.GetUnixTimestamp(), true);
            _roles[0] = role;
            SaveRoles();

            return role;
        }

        public void MakeAdmin(int id)
        {
            if (!IsMember(id) || id == CreatorId)
                return;

            int targetLevel = _roles.Keys.Where(level => level > 1).DefaultIfEmpty(0).Min();
            if (targetLevel <= 1)
                targetLevel = AddRole("Administrator", 0, string.Empty, string.Empty, 5, true).Level;

            UpdateMemberLevel(id, targetLevel);
        }

        public void TakeAdmin(int userId)
        {
            if (!IsAdmin(userId) || userId == CreatorId)
                return;

            UpdateMemberLevel(userId, 1);
        }

        public bool TryPeekPromotion(int userId, out GroupRole nextRole)
        {
            nextRole = null;

            if (userId == CreatorId || !_memberLevels.TryGetValue(userId, out int currentLevel) || currentLevel < 1)
                return false;

            nextRole = _roles.Values
                .Where(role => role.Level > currentLevel)
                .OrderBy(role => role.Level)
                .FirstOrDefault();

            return nextRole != null;
        }

        public bool TryPromoteMember(int userId, out GroupRole nextRole)
        {
            if (!TryPeekPromotion(userId, out nextRole))
                return false;

            UpdateMemberLevel(userId, nextRole.Level);
            return true;
        }

        public bool TryDemoteMember(int userId, out GroupRole nextRole)
        {
            nextRole = null;

            if (userId == CreatorId || !_memberLevels.TryGetValue(userId, out int currentLevel) || currentLevel < 1)
                return false;

            nextRole = _roles.Values
                .Where(role => role.Level >= 1 && role.Level < currentLevel)
                .OrderByDescending(role => role.Level)
                .FirstOrDefault();

            if (nextRole == null)
                return false;

            UpdateMemberLevel(userId, nextRole.Level);
            return true;
        }

        public void AddMember(int id, int level = 1)
        {
            if (IsMember(id) || Type == GroupType.Locked && _requests.Contains(id))
                return;

            if (Type == GroupType.Locked)
                _requests.Add(id);
            else
                _memberLevels[id] = level;

            MarkDirty();
        }

        public void DeleteMember(int id)
        {
            if (id == CreatorId || !IsMember(id))
                return;

            _memberLevels.TryRemove(id, out _);
            MarkDirty();

            ClearEmployment(id);
            ClearFavourite(id);

            // Leaving/being kicked from a gang drops the gang tag on the member's motto
            // (persists even if they're offline). Covers every removal path centrally.
            if (GroupManager.IsGangKind(Kind))
                Roleplay.Utilities.GangMottoService.ClearToCitizen(id);
        }

        private void ClearFavourite(int userId)
        {
            int groupId = Id;

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.UserStats.Where(u => u.Id == userId && u.Groupid == groupId)
                    .ExecuteUpdate(s => s.SetProperty(u => u.Groupid, 0));
            }

            Users.Habbo habbo = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId)?.GetHabbo();
            if (habbo?.GetStats() == null || habbo.GetStats().FavouriteGroupId != groupId)
                return;

            habbo.GetStats().FavouriteGroupId = 0;

            if (habbo.InRoom && habbo.CurrentRoom != null) {
                RoomUser user = habbo.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(userId);
                if (user != null)
                    habbo.CurrentRoom.SendPacket(new Communication.Packets.Outgoing.Groups.UpdateFavouriteGroupComposer(this, user.VirtualId));

                habbo.CurrentRoom.SendPacket(new Communication.Packets.Outgoing.Groups.RefreshFavouriteGroupComposer(userId));
            } else
                habbo.GetClient()?.SendPacket(new Communication.Packets.Outgoing.Groups.RefreshFavouriteGroupComposer(userId));
        }

        private void ClearEmployment(int userId)
        {
            if (!GroupManager.IsWorkableKind(Kind))
                return;

            Users.Habbo habbo = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId)?.GetHabbo();
            if (habbo == null || habbo.CorporationId != Id)
                return;

            habbo.CorporationId = 0;

            if (PlusEnvironment.GetGame().GetShiftManager().IsWorkingFor(userId, Id))
                PlusEnvironment.GetGame().GetShiftManager().InterruptShift(habbo, habbo.CurrentRoom);
        }

        public void HandleRequest(int id, bool accepted)
        {
            if (accepted)
                _memberLevels[id] = 1;

            _requests.Remove(id);

            MarkDirty();
        }

        public RoomData GetRoom()
        {
            if (_room == null) {
                if (!RoomFactory.TryGetData(RoomId, out RoomData data))
                    return null;

                _room = data;
                return data;
            }

            return _room;
        }

        public bool UpdateRole(int level, string name, int shiftPay, string shiftCostume, int shiftDuration, string shiftMotto)
        {
            if (!_roles.TryGetValue(level, out GroupRole role))
                return false;

            NormalizeRoleValues(ref shiftPay, ref shiftCostume, ref shiftDuration, ref shiftMotto);
            role.Update(name, shiftPay, shiftCostume, shiftDuration, shiftMotto);
            return true;
        }

        public GroupRole AddRole(string name, int shiftPay, string shiftCostume, string shiftMotto, int shiftDuration, bool saveImmediately = false)
        {
            int level = _roles.Keys.DefaultIfEmpty(0).Max() + 1;
            NormalizeRoleValues(ref shiftPay, ref shiftCostume, ref shiftDuration, ref shiftMotto);

            GroupRole role = new(0, Id, level, name, shiftPay, shiftCostume, shiftDuration, shiftMotto, (int)PlusEnvironment.GetUnixTimestamp(), true);
            _roles[level] = role;

            if (saveImmediately)
                SaveRoles();

            return role;
        }

        public bool RemoveRole(int actorUserId, int level)
        {
            if (actorUserId != CreatorId || level <= 1 || !_roles.TryRemove(level, out GroupRole role))
                return false;

            foreach (int userId in _memberLevels.Where(x => x.Value == level).Select(x => x.Key).ToList())
                UpdateMemberLevel(userId, 1);

            if (role.Id > 0)
                _deletedRoleIds.Add(role.Id);

            return true;
        }

        public bool RemoveGangRole(int level)
        {
            if (level <= 1 || !_roles.TryRemove(level, out GroupRole role))
                return false;

            foreach (int userId in _memberLevels.Where(x => x.Value == level).Select(x => x.Key).ToList())
                UpdateMemberLevel(userId, 1);

            if (role.Id > 0)
                _deletedRoleIds.Add(role.Id);

            return true;
        }

        public void SaveRoles()
        {
            if (_deletedRoleIds.Count == 0 && _roles.Values.All(x => !x.Dirty && !x.IsNew))
                return;

            using WavePlusContext db = PlusEnvironment.GetDbContext();

            foreach (int deletedRoleId in _deletedRoleIds.ToList()) {
                int rid = deletedRoleId;
                db.GroupRoles.Where(r => r.Id == rid).ExecuteDelete();
            }

            _deletedRoleIds.Clear();

            foreach (GroupRole role in _roles.Values.OrderBy(x => x.Level)) {
                if (role.IsNew) {
                    var entity = new Database.EF.Entities.GroupRoleEntity
                    {
                        GroupId = role.GroupId,
                        Level = role.Level,
                        Name = role.Name,
                        ShiftPay = role.ShiftPay,
                        ShiftCostume = role.ShiftCostume,
                        ShiftDuration = (short)role.ShiftDuration,
                        ShiftMotto = role.ShiftMotto,
                        CreatedAt = role.CreatedAt
                    };
                    db.GroupRoles.Add(entity);
                    db.SaveChanges();
                    role.MarkPersisted(entity.Id);
                    continue;
                }

                if (!role.Dirty)
                    continue;

                int roleId = role.Id;
                string name = role.Name;
                int shiftPay = role.ShiftPay;
                string shiftCostume = role.ShiftCostume;
                short shiftDuration = (short)role.ShiftDuration;
                string shiftMotto = role.ShiftMotto;
                db.GroupRoles.Where(r => r.Id == roleId).ExecuteUpdate(s => s
                    .SetProperty(r => r.Name, name)
                    .SetProperty(r => r.ShiftPay, shiftPay)
                    .SetProperty(r => r.ShiftCostume, shiftCostume)
                    .SetProperty(r => r.ShiftDuration, shiftDuration)
                    .SetProperty(r => r.ShiftMotto, shiftMotto));
                role.MarkSaved();
            }
        }

        public void SavePermissions()
        {
            if (_deletedPermissionIds.Count == 0 && _permissions.Values.All(x => !x.IsNew))
                return;

            using WavePlusContext db = PlusEnvironment.GetDbContext();

            foreach (int deletedPermissionId in _deletedPermissionIds.ToList()) {
                int pid = deletedPermissionId;
                db.GroupPermissions.Where(p => p.Id == pid).ExecuteDelete();
            }

            _deletedPermissionIds.Clear();

            foreach (GroupPermissionAssignment assignment in _permissions.Values.ToList()) {
                if (!assignment.IsNew)
                    continue;

                var entity = new Database.EF.Entities.GroupPermissionEntity
                {
                    GroupId = assignment.GroupId,
                    LevelId = assignment.LevelId,
                    PermissionId = assignment.PermissionId,
                    CreatedAt = assignment.CreatedAt
                };
                db.GroupPermissions.Add(entity);
                db.SaveChanges();
                assignment.MarkPersisted(entity.Id);
            }
        }

        public void ClearRequests()
        {
            _requests.Clear();
        }

        public void MarkDirty()
        {
            _dirty = true;
        }

        public IReadOnlyDictionary<int, int> MemberLevelsSnapshot => new Dictionary<int, int>(_memberLevels);

        public void Flush()
        {
            SaveRoles();
            SavePermissions();

            if (_dirty) {
                uint gid = (uint)Id;
                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    // Oracle's MySql.EntityFrameworkCore mistranslates ExecuteUpdate/ExecuteDelete
                    // against MariaDB (it version-sniffs for MySQL 8 syntax), which crashes world save
                    // and silently drops membership writes. Raw parameterized SQL goes straight to the
                    // connector, so it works on both MySQL and MariaDB. One transaction keeps the
                    // delete + re-insert atomic.
                    using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction tx = db.Database.BeginTransaction();

                    string state = ((int)Type).ToString();
                    string admindeco = AdminOnlyDeco.ToString();
                    string forumEnabled = ForumEnabled ? "1" : "0";
                    short groupType = (short)Kind;

                    db.Database.ExecuteSqlInterpolated($@"UPDATE `groups` SET
                        `name` = {Name},
                        `desc` = {Description},
                        `badge` = {Badge},
                        `colour1` = {Colour1},
                        `colour2` = {Colour2},
                        `state` = {state},
                        `admindeco` = {admindeco},
                        `forum_enabled` = {forumEnabled},
                        `group_type` = {groupType}
                        WHERE `id` = {gid}");

                    db.Database.ExecuteSqlInterpolated($"DELETE FROM `group_memberships` WHERE `group_id` = {gid}");
                    foreach (KeyValuePair<int, int> kv in _memberLevels) {
                        uint memberId = (uint)kv.Key;
                        short level = (short)kv.Value;
                        db.Database.ExecuteSqlInterpolated(
                            $"INSERT INTO `group_memberships` (`group_id`, `user_id`, `level`) VALUES ({gid}, {memberId}, {level})");
                    }

                    db.Database.ExecuteSqlInterpolated($"DELETE FROM `group_requests` WHERE `group_id` = {gid}");
                    foreach (int uid in _requests) {
                        uint requestUserId = (uint)uid;
                        db.Database.ExecuteSqlInterpolated(
                            $"INSERT INTO `group_requests` (`group_id`, `user_id`) VALUES ({gid}, {requestUserId})");
                    }

                    tx.Commit();
                }

                _dirty = false;
            }

            Plus.Core.Cache.GroupCache.Mirror(this);
        }

        public void Dispose()
        {
            // Memory-only teardown (used on group deletion). Persistence happens on the 15-minute
            // SaveAll/Flush path; flushing here would re-insert the rows a deletion just removed.
            _requests.Clear();
            _memberLevels.Clear();
            _roles.Clear();
            _deletedRoleIds.Clear();
            _permissions.Clear();
            _deletedPermissionIds.Clear();
        }

        private void UpdateMemberLevel(int userId, int level)
        {
            if (!_memberLevels.ContainsKey(userId))
                return;

            _memberLevels[userId] = level;
            MarkDirty();
        }

        private void NormalizeRoleValues(ref int shiftPay, ref string shiftCostume, ref int shiftDuration, ref string shiftMotto)
        {
            shiftPay = Math.Max(0, shiftPay);
            shiftDuration = shiftDuration <= 0 ? 5 : shiftDuration;
            shiftCostume ??= string.Empty;
            shiftMotto ??= string.Empty;
        }

        private static string BuildPermissionKey(int levelId, int permissionId)
        {
            return levelId + ":" + permissionId;
        }
    }
}