using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.Groups;

namespace Plus.Core.Cache
{
    public static class GroupCache
    {
        public const string IndexSet = "rp:groups:ids";

        private static string Key(int groupId) => $"rp:group:{groupId}";

        private sealed class GroupSnapshot
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Badge { get; set; }
            public int Colour1 { get; set; }
            public int Colour2 { get; set; }
            public int RoomId { get; set; }
            public int OwnerId { get; set; }
            public int Type { get; set; }
            public int Kind { get; set; }
            public int AdminOnlyDeco { get; set; }
            public bool ForumEnabled { get; set; }
            public Dictionary<int, int> Members { get; set; }
            public List<int> Requests { get; set; }
            public List<RoleSnapshot> Roles { get; set; }
            public List<string> Permissions { get; set; }
        }

        private sealed class RoleSnapshot
        {
            public int Level { get; set; }
            public string Name { get; set; }
            public int ShiftPay { get; set; }
            public string ShiftCostume { get; set; }
            public int ShiftDuration { get; set; }
            public string ShiftMotto { get; set; }
        }

        public static void Mirror(Group group)
        {
            if (group == null)
                return;

            RedisManager redis = PlusEnvironment.GetRedis();
            if (redis == null || !redis.IsAvailable)
                return;

            var snapshot = new GroupSnapshot
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                Badge = group.Badge,
                Colour1 = group.Colour1,
                Colour2 = group.Colour2,
                RoomId = group.RoomId,
                OwnerId = group.CreatorId,
                Type = (int)group.Type,
                Kind = (int)group.Kind,
                AdminOnlyDeco = group.AdminOnlyDeco,
                ForumEnabled = group.ForumEnabled,
                Members = new Dictionary<int, int>(group.MemberLevelsSnapshot),
                Requests = group.GetRequests,
                Roles = [.. group.GetRoles.Select(r => new RoleSnapshot
                {
                    Level = r.Level,
                    Name = r.Name,
                    ShiftPay = r.ShiftPay,
                    ShiftCostume = r.ShiftCostume,
                    ShiftDuration = r.ShiftDuration,
                    ShiftMotto = r.ShiftMotto
                })],
                Permissions = [.. group.GetPermissions.Select(p => p.LevelId + ":" + p.PermissionId)]
            };

            redis.SetObject(Key(group.Id), snapshot);
            redis.SetAdd(IndexSet, group.Id.ToString());
        }

        public static void MirrorAll()
        {
            RedisManager redis = PlusEnvironment.GetRedis();
            if (redis == null || !redis.IsAvailable)
                return;

            foreach (Group group in PlusEnvironment.GetGame().GetGroupManager().GetLoadedGroups())
                Mirror(group);
        }

        public static void Remove(int groupId)
        {
            RedisManager redis = PlusEnvironment.GetRedis();
            if (redis == null || !redis.IsAvailable)
                return;

            redis.Delete(Key(groupId));
            redis.SetRemove(IndexSet, groupId.ToString());
        }
    }
}