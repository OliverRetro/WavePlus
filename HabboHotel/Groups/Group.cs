using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Plus.Database.Interfaces;
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

        private RoomData _room;

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
            Type = (GroupType) type;
            Kind = (GroupKind) groupType;

            AdminOnlyDeco = adminOnlyDeco;
            ForumEnabled = ForumEnabled;

            _memberLevels = new ConcurrentDictionary<int, int>();
            _requests = new List<int>();
            _roles = new ConcurrentDictionary<int, GroupRole>();
            _deletedRoleIds = new List<int>();
            _permissions = new ConcurrentDictionary<string, GroupPermissionAssignment>(StringComparer.OrdinalIgnoreCase);
            _deletedPermissionIds = new List<int>();

            if (Id > 0)
            {
                InitMembers();
                InitRoles();
                InitPermissions();
            }
        }

        public void InitMembers()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `user_id`, `level` FROM `group_memberships` WHERE `group_id` = @id");
                dbClient.AddParameter("id", Id);
                DataTable members = dbClient.GetTable();

                if (members != null)
                {
                    foreach (DataRow row in members.Rows)
                    {
                        int userId = Convert.ToInt32(row["user_id"]);
                        int level = Convert.ToInt32(row["level"]);
                        if (userId == CreatorId)
                            level = 0;

                        _memberLevels[userId] = level;

                        if (userId == CreatorId && Convert.ToInt32(row["level"]) != 0)
                        {
                            dbClient.SetQuery("UPDATE `group_memberships` SET `level` = '0' WHERE `user_id` = @uid AND `group_id` = @gid LIMIT 1");
                            dbClient.AddParameter("uid", userId);
                            dbClient.AddParameter("gid", Id);
                            dbClient.RunQuery();
                        }
                    }
                }

                if (!_memberLevels.ContainsKey(CreatorId))
                {
                    dbClient.SetQuery("INSERT INTO `group_memberships` (`user_id`, `group_id`, `level`) VALUES (@uid, @gid, '0')");
                    dbClient.AddParameter("uid", CreatorId);
                    dbClient.AddParameter("gid", Id);
                    dbClient.RunQuery();
                    _memberLevels[CreatorId] = 0;
                }

                dbClient.SetQuery("SELECT `user_id` FROM `group_requests` WHERE `group_id` = @id");
                dbClient.AddParameter("id", Id);
                DataTable requests = dbClient.GetTable();

                if (requests != null)
                {
                    foreach (DataRow row in requests.Rows)
                    {
                        int userId = Convert.ToInt32(row["user_id"]);

                        if (_memberLevels.ContainsKey(userId))
                        {
                            dbClient.RunQuery("DELETE FROM `group_requests` WHERE `group_id` = '" + Id + "' AND `user_id` = '" + userId + "'");
                        }
                        else if (!_requests.Contains(userId))
                        {
                            _requests.Add(userId);
                        }
                    }
                }
            }
        }

        public void InitRoles()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`, `group_id`, `level`, `name`, `shift_pay`, `shift_costume`, `shift_duration`, `shift_motto`, `created_at` FROM `group_roles` WHERE `group_id` = @group_id ORDER BY `level` ASC");
                dbClient.AddParameter("group_id", Id);
                DataTable roles = dbClient.GetTable();

                if (roles != null)
                {
                    foreach (DataRow row in roles.Rows)
                    {
                        GroupRole role = new GroupRole(
                            Convert.ToInt32(row["id"]),
                            Convert.ToInt32(row["group_id"]),
                            Convert.ToInt32(row["level"]),
                            Convert.ToString(row["name"]),
                            Convert.ToInt32(row["shift_pay"]),
                            Convert.ToString(row["shift_costume"]),
                            row["shift_duration"] == DBNull.Value ? 5 : Convert.ToInt32(row["shift_duration"]),
                            Convert.ToString(row["shift_motto"]),
                            Convert.ToDateTime(row["created_at"]));

                        _roles[role.Level] = role;
                    }
                }
            }

        }

        public void InitPermissions()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`, `group_id`, `level_id`, `permission_id`, `created_at` FROM `group_permissions` WHERE `group_id` = @group_id");
                dbClient.AddParameter("group_id", Id);
                DataTable permissions = dbClient.GetTable();

                if (permissions == null)
                    return;

                foreach (DataRow row in permissions.Rows)
                {
                    GroupPermissionAssignment assignment = new GroupPermissionAssignment(
                        Convert.ToInt32(row["id"]),
                        Convert.ToInt32(row["group_id"]),
                        Convert.ToInt32(row["level_id"]),
                        Convert.ToInt32(row["permission_id"]),
                        Convert.ToDateTime(row["created_at"]));

                    _permissions[BuildPermissionKey(assignment.LevelId, assignment.PermissionId)] = assignment;
                }
            }
        }

        public List<int> GetMembers => _memberLevels.Where(x => x.Value == 1).Select(x => x.Key).ToList();

        public List<int> GetRequests => _requests.ToList();

        public List<int> GetAdministrators => _memberLevels.Where(x => x.Key == CreatorId || x.Value > 1).Select(x => x.Key).ToList();

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
                return _memberLevels.Keys.ToList();
            }
        }

        public int MemberCount => _memberLevels.Count;

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
            if (!permission.SupportsGroupKind(Kind))
            {
                if (!enabled)
                    return true;

                return false;
            }

            if (!enabled)
            {
                if (!_permissions.TryRemove(assignmentKey, out GroupPermissionAssignment removed))
                    return true;

                if (removed.Id > 0)
                    _deletedPermissionIds.Add(removed.Id);

                return true;
            }

            if (_permissions.ContainsKey(assignmentKey))
                return true;

            _permissions[assignmentKey] = new GroupPermissionAssignment(0, Id, levelId, permissionId, DateTime.UtcNow, true);
            return true;
        }

        public int GetMemberLevel(int id)
        {
            return _memberLevels.TryGetValue(id, out int level) ? level : -1;
        }

        public string GetMemberRoleName(int id)
        {
            if (!_memberLevels.TryGetValue(id, out int level))
                return string.Empty;

            if (id == CreatorId || level == 0)
                return "Owner";

            return _roles.TryGetValue(level, out GroupRole role) ? role.Name : level.ToString();
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

        public bool TryPromoteMember(int userId, out GroupRole nextRole)
        {
            nextRole = null;

            if (userId == CreatorId || !_memberLevels.TryGetValue(userId, out int currentLevel) || currentLevel < 1)
                return false;

            nextRole = _roles.Values
                .Where(role => role.Level > currentLevel)
                .OrderBy(role => role.Level)
                .FirstOrDefault();

            if (nextRole == null)
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
                .Where(role => role.Level < currentLevel)
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

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                if (Type == GroupType.Locked)
                {
                    dbClient.SetQuery("INSERT INTO `group_requests` (user_id, group_id) VALUES (@uid, @gid)");
                    _requests.Add(id);
                }
                else
                {
                    dbClient.SetQuery("INSERT INTO `group_memberships` (`user_id`, `group_id`, `level`) VALUES (@uid, @gid, @rank)");
                    dbClient.AddParameter("rank", level);
                }

                dbClient.AddParameter("gid", Id);
                dbClient.AddParameter("uid", id);
                dbClient.RunQuery();
            }

            if (Type != GroupType.Locked)
                _memberLevels[id] = level;
        }

        public void DeleteMember(int id)
        {
            if (id == CreatorId || !IsMember(id))
                return;

            _memberLevels.TryRemove(id, out _);

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("DELETE FROM group_memberships WHERE user_id=@uid AND group_id=@gid LIMIT 1");
                dbClient.AddParameter("gid", Id);
                dbClient.AddParameter("uid", id);
                dbClient.RunQuery();
            }
        }

        public void HandleRequest(int id, bool accepted)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                if (accepted)
                {
                    dbClient.SetQuery("INSERT INTO `group_memberships` (`user_id`, `group_id`, `level`) VALUES (@uid, @gid, 1)");
                    dbClient.AddParameter("gid", Id);
                    dbClient.AddParameter("uid", id);
                    dbClient.RunQuery();

                    _memberLevels[id] = 1;
                }

                dbClient.SetQuery("DELETE FROM group_requests WHERE user_id=@uid AND group_id=@gid LIMIT 1");
                dbClient.AddParameter("gid", Id);
                dbClient.AddParameter("uid", id);
                dbClient.RunQuery();
            }

            if (_requests.Contains(id))
                _requests.Remove(id);
        }

        public RoomData GetRoom()
        {
            if (_room == null)
            {
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

            GroupRole role = new GroupRole(0, Id, level, name, shiftPay, shiftCostume, shiftDuration, shiftMotto, DateTime.UtcNow, true);
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

        public void SaveRoles()
        {
            if (_deletedRoleIds.Count == 0 && _roles.Values.All(x => !x.Dirty && !x.IsNew))
                return;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                foreach (int deletedRoleId in _deletedRoleIds.ToList())
                {
                    dbClient.SetQuery("DELETE FROM `group_roles` WHERE `id` = @id LIMIT 1");
                    dbClient.AddParameter("id", deletedRoleId);
                    dbClient.RunQuery();
                }

                _deletedRoleIds.Clear();

                foreach (GroupRole role in _roles.Values.OrderBy(x => x.Level))
                {
                    if (role.IsNew)
                    {
                        dbClient.SetQuery("INSERT INTO `group_roles` (`group_id`, `level`, `name`, `shift_pay`, `shift_costume`, `shift_duration`, `shift_motto`, `created_at`) VALUES (@group_id, @level, @name, @shift_pay, @shift_costume, @shift_duration, @shift_motto, @created_at)");
                        dbClient.AddParameter("group_id", role.GroupId);
                        dbClient.AddParameter("level", role.Level);
                        dbClient.AddParameter("name", role.Name);
                        dbClient.AddParameter("shift_pay", role.ShiftPay);
                        dbClient.AddParameter("shift_costume", role.ShiftCostume);
                        dbClient.AddParameter("shift_duration", role.ShiftDuration);
                        dbClient.AddParameter("shift_motto", role.ShiftMotto);
                        dbClient.AddParameter("created_at", role.CreatedAt);
                        role.MarkPersisted(Convert.ToInt32(dbClient.InsertQuery()));
                        continue;
                    }

                    if (!role.Dirty)
                        continue;

                    dbClient.SetQuery("UPDATE `group_roles` SET `name` = @name, `shift_pay` = @shift_pay, `shift_costume` = @shift_costume, `shift_duration` = @shift_duration, `shift_motto` = @shift_motto WHERE `id` = @id LIMIT 1");
                    dbClient.AddParameter("id", role.Id);
                    dbClient.AddParameter("name", role.Name);
                    dbClient.AddParameter("shift_pay", role.ShiftPay);
                    dbClient.AddParameter("shift_costume", role.ShiftCostume);
                    dbClient.AddParameter("shift_duration", role.ShiftDuration);
                    dbClient.AddParameter("shift_motto", role.ShiftMotto);
                    dbClient.RunQuery();
                    role.MarkSaved();
                }
            }
        }

        public void SavePermissions()
        {
            if (_deletedPermissionIds.Count == 0 && _permissions.Values.All(x => !x.IsNew))
                return;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                foreach (int deletedPermissionId in _deletedPermissionIds.ToList())
                {
                    dbClient.SetQuery("DELETE FROM `group_permissions` WHERE `id` = @id LIMIT 1");
                    dbClient.AddParameter("id", deletedPermissionId);
                    dbClient.RunQuery();
                }

                _deletedPermissionIds.Clear();

                foreach (GroupPermissionAssignment assignment in _permissions.Values.ToList())
                {
                    if (!assignment.IsNew)
                        continue;

                    dbClient.SetQuery("INSERT INTO `group_permissions` (`group_id`, `level_id`, `permission_id`, `created_at`) VALUES (@group_id, @level_id, @permission_id, @created_at)");
                    dbClient.AddParameter("group_id", assignment.GroupId);
                    dbClient.AddParameter("level_id", assignment.LevelId);
                    dbClient.AddParameter("permission_id", assignment.PermissionId);
                    dbClient.AddParameter("created_at", assignment.CreatedAt);
                    assignment.MarkPersisted(Convert.ToInt32(dbClient.InsertQuery()));
                }
            }
        }


        public void ClearRequests()
        {
            _requests.Clear();
        }

        public void Dispose()
        {
            SaveRoles();
            SavePermissions();
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

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `group_memberships` SET `level` = @level WHERE `user_id` = @uid AND `group_id` = @gid LIMIT 1");
                dbClient.AddParameter("level", level);
                dbClient.AddParameter("gid", Id);
                dbClient.AddParameter("uid", userId);
                dbClient.RunQuery();
            }

            _memberLevels[userId] = level;
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
