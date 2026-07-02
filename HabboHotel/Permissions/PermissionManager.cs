using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using log4net;
using Plus.Database.Interfaces;
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

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `permissions`");
                DataTable getPermissions = dbClient.GetTable();

                if (getPermissions != null) {
                    foreach (DataRow row in getPermissions.Rows)
                    {
                        _permissions.Add(Convert.ToInt32(row["id"]), new Permission(Convert.ToInt32(row["id"]), Convert.ToString(row["permission"]), Convert.ToString(row["description"])));
                    }
                }
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `permissions_commands`");
                DataTable getCommands = dbClient.GetTable();

                if (getCommands != null) {
                    foreach (DataRow row in getCommands.Rows)
                    {
                        _commands.Add(Convert.ToString(row["command"]), new PermissionCommand(Convert.ToString(row["command"]), Convert.ToInt32(row["group_id"]), Convert.ToInt32(row["subscription_id"])));
                    }
                }
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `permissions_groups`");
                DataTable getPermissionGroups = dbClient.GetTable();

                if (getPermissionGroups != null) {
                    foreach (DataRow row in getPermissionGroups.Rows)
                    {
                        _permissionGroups.Add(Convert.ToInt32(row["id"]), new PermissionGroup(Convert.ToString("name"), Convert.ToString("description"), Convert.ToString("badge")));
                    }
                }
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `permissions_rights`");
                DataTable getPermissionRights = dbClient.GetTable();

                if (getPermissionRights != null) {
                    foreach (DataRow row in getPermissionRights.Rows)
                    {
                        int groupId = Convert.ToInt32(row["group_id"]);
                        int permissionId = Convert.ToInt32(row["permission_id"]);

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
                }
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `permissions_subscriptions`");
                DataTable getPermissionSubscriptions = dbClient.GetTable();

                if (getPermissionSubscriptions != null) {
                    foreach (DataRow row in getPermissionSubscriptions.Rows)
                    {
                        int permissionId = Convert.ToInt32(row["permission_id"]);
                        int subscriptionId = Convert.ToInt32(row["subscription_id"]);

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