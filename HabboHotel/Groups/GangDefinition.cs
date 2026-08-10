using System.Collections.Generic;
using System.Linq;

namespace Plus.HabboHotel.Groups
{
    public static class GangDefinition
    {
        public const int PermEditRoom = 1;    // edit_room / Decorate — excluded for gangs
        public const int PermPromote = 3;     // promote_user — promote a member up a role
        public const int PermKick = 4;        // kick_from_group — remove a member entirely
        public const int PermEditGroup = 5;   // edit_group — edit colours/settings
        public const int PermEditLevels = 6;  // edit_levels — manage (add/edit/delete) roles
        public const int PermDemote = 7;      // demote_players — demote a member down a role
        public const int PermGangInvite = 14; // gang_invite — invite members

        private static readonly Dictionary<GroupKind, int> DefaultLimits = new()
        {
            { GroupKind.Gang, 20 },
            { GroupKind.Cartel, 35 },
            { GroupKind.Mafia, 50 }
        };

        // Creation gates: credit cost + knockouts (kills) required, per organization kind.
        private static readonly Dictionary<GroupKind, int> DefaultCreationCost = new()
        {
            { GroupKind.Gang, 100 },
            { GroupKind.Mafia, 500 },
            { GroupKind.Cartel, 1000 }
        };

        private static readonly Dictionary<GroupKind, int> DefaultKillsRequired = new()
        {
            { GroupKind.Gang, 10 },
            { GroupKind.Mafia, 50 },
            { GroupKind.Cartel, 100 }
        };

        public static readonly GroupKind[] CreatableKinds = { GroupKind.Gang, GroupKind.Mafia, GroupKind.Cartel };

        // The tier upgrade chain: Gang -> Mafia -> Cartel. Cartel is the top tier.
        public static readonly GroupKind[] UpgradeChain = { GroupKind.Gang, GroupKind.Mafia, GroupKind.Cartel };

        // Fallback badge every new gang starts with (same code normal groups fall back to).
        public const string DefaultBadge = "b05114s06114";

        // The next tier up from the given kind, or null if already at the top.
        public static GroupKind? NextKind(GroupKind kind)
        {
            for (int i = 0; i < UpgradeChain.Length - 1; i++) {
                if (UpgradeChain[i] == kind)
                    return UpgradeChain[i + 1];
            }
            return null;
        }

        public static string DisplayName(GroupKind kind) => kind switch
        {
            GroupKind.Gang => "Gang",
            GroupKind.Cartel => "Cartel",
            GroupKind.Mafia => "Mafia",
            _ => "Gang"
        };

        public static int MemberLimit(GroupKind kind)
        {
            string key = "gang.memberlimit." + DisplayName(kind).ToLowerInvariant();
            if (TryGetConfigInt(key, out int limit))
                return limit;

            return DefaultLimits.TryGetValue(kind, out int fallback) ? fallback : 20;
        }

        public static int CreationCost(GroupKind kind)
        {
            string key = "gang.creation.cost." + DisplayName(kind).ToLowerInvariant();
            if (TryGetConfigInt(key, out int cost))
                return cost;

            return DefaultCreationCost.TryGetValue(kind, out int fallback) ? fallback : 100;
        }

        public static int KillsRequired(GroupKind kind)
        {
            string key = "gang.creation.kills." + DisplayName(kind).ToLowerInvariant();
            if (TryGetConfigInt(key, out int kills))
                return kills;

            return DefaultKillsRequired.TryGetValue(kind, out int fallback) ? fallback : 0;
        }

        public static List<GroupPermissionKey> ApplicableKeys(GroupKind kind)
        {
            return PlusEnvironment.GetGame().GetGroupManager().PermissionKeys
                .Where(k => k.Id != PermEditRoom && k.SupportsGroupKind(kind))
                .OrderBy(k => k.Id)
                .ToList();
        }

        private static bool TryGetConfigInt(string key, out int value)
        {
            value = 0;
            var config = PlusEnvironment.GetConfig();
            return config?.Data != null
                && config.Data.TryGetValue(key, out string raw)
                && int.TryParse(raw, out value);
        }
    }
}