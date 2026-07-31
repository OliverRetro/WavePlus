using System.Collections.Generic;
using System.Globalization;
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

        public static int CreationCost()
        {
            return TryGetConfigInt("gang.creation.cost", out int cost) ? cost : 300;
        }

        public static List<GroupPermissionKey> ApplicableKeys(GroupKind kind)
        {
            return PlusEnvironment.GetGame().GetGroupManager().PermissionKeys
                .Where(k => k.Id != PermEditRoom && k.SupportsGroupKind(kind))
                .OrderBy(k => k.Id)
                .ToList();
        }

        // ── Colour packing (native colour1/colour2 int columns store packed RGB) ──
        public static string ToHex(int colour)
        {
            return "#" + (colour & 0xFFFFFF).ToString("X6");
        }

        public static int ToInt(string hex)
        {
            hex = (hex ?? string.Empty).TrimStart('#');
            if (hex.Length == 6 && int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int value))
                return value;

            return 0;
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
