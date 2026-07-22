using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Plus.Database.EF;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Permissions
{
    public sealed class PermissionManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PermissionManager));

        private readonly Dictionary<int, Permission> _permissions = new();

        private readonly Dictionary<string, PermissionCommand> _commands = new();

        private readonly Dictionary<int, PermissionGroup> _permissionGroups = new();

        private readonly Dictionary<int, List<string>> _permissionGroupRights = new();

        private readonly Dictionary<int, List<string>> _permissionSubscriptionRights = new();

        public void Init()
        {
            _permissions.Clear();
            _commands.Clear();
            _permissionGroups.Clear();
            _permissionGroupRights.Clear();

            using WavePlusContext db = PlusEnvironment.GetDbContext();

            foreach (var row in db.Permissions.Select(p => new { p.Id, p.Permission1, p.Description }).ToList())
                _permissions.Add(row.Id, new Permission(row.Id, row.Permission1, row.Description));

            foreach (var row in db.PermissionsCommands.Select(c => new { c.Command, c.GroupId, c.SubscriptionId }).ToList())
                _commands.Add(row.Command, new PermissionCommand(row.Command, row.GroupId, row.SubscriptionId));

            foreach (var row in db.PermissionsGroups.Select(g => new { g.Id }).ToList())
                _permissionGroups.Add(row.Id, new PermissionGroup(Convert.ToString("name"), Convert.ToString("description"), Convert.ToString("badge")));

            foreach (var row in db.PermissionsRights.Select(r => new { r.GroupId, r.PermissionId }).ToList()) {
                int groupId = row.GroupId;
                int permissionId = row.PermissionId;

                if (!_permissionGroups.ContainsKey(groupId)) {
                    continue; // permission group does not exist
                }

                if (!_permissions.TryGetValue(permissionId, out Permission permission)) {
                    continue; // permission does not exist
                }

                if (_permissionGroupRights.ContainsKey(groupId)) {
                    _permissionGroupRights[groupId].Add(permission.PermissionName);
                } else {
                    List<string> rightsSet = new()
                    {
                        permission.PermissionName
                    };

                    _permissionGroupRights.Add(groupId, rightsSet);
                }
            }

            foreach (var row in db.PermissionsSubscriptions.Select(sub => new { sub.PermissionId, sub.SubscriptionId }).ToList()) {
                int permissionId = row.PermissionId;
                int subscriptionId = row.SubscriptionId;

                if (!_permissions.TryGetValue(permissionId, out Permission permission))
                    continue; // permission does not exist

                if (_permissionSubscriptionRights.ContainsKey(subscriptionId)) {
                    _permissionSubscriptionRights[subscriptionId].Add(permission.PermissionName);
                } else {
                    List<string> rightsSet = new()
                    {
                        permission.PermissionName
                    };

                    _permissionSubscriptionRights.Add(subscriptionId, rightsSet);
                }
            }

            Log.Info("Loaded " + _permissions.Count + " permissions.");
            Log.Info("Loaded " + _permissionGroups.Count + " permissions groups.");
            Log.Info("Loaded " + _permissionGroupRights.Count + " permissions group rights.");
            Log.Info("Loaded " + _permissionSubscriptionRights.Count + " permissions subscription rights.");
        }

        public bool TryGetGroup(int id, out PermissionGroup group)
        {
            return _permissionGroups.TryGetValue(id, out group);
        }

        public List<string> GetPermissionsForPlayer(Habbo player)
        {
            List<string> permissionSet = new();

            if (_permissionGroupRights.TryGetValue(player.Rank, out List<string> permRights)) {
                permissionSet.AddRange(permRights);
            }

            if (_permissionSubscriptionRights.TryGetValue(player.VipRank, out List<string> subscriptionRights)) {
                permissionSet.AddRange(subscriptionRights);
            }

            return permissionSet;
        }

        public List<string> GetCommandsForPlayer(Habbo player)
        {
            return _commands.Where(x => player.Rank >= x.Value.GroupId && player.VipRank >= x.Value.SubscriptionId).Select(x => x.Key).ToList();
        }
    }
}