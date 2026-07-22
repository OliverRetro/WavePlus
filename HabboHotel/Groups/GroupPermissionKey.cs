using System;
using System.Collections.Generic;

namespace Plus.HabboHotel.Groups
{
    public class GroupPermissionKey
    {
        public int Id { get; }
        public string Name { get; }
        public string Key { get; }
        public string Description { get; }
        private readonly HashSet<GroupKind> _typeSpecific;

        public GroupPermissionKey(int id, string name, string key, string description, string typeSpecific)
        {
            Id = id;
            Name = name ?? string.Empty;
            Key = key ?? string.Empty;
            Description = description ?? string.Empty;
            _typeSpecific = ParseTypeSpecific(typeSpecific);
        }

        public bool SupportsGroupKind(GroupKind kind)
        {
            return _typeSpecific.Contains(kind);
        }

        public ICollection<GroupKind> TypeSpecific => [.. _typeSpecific];

        private static HashSet<GroupKind> ParseTypeSpecific(string raw)
        {
            HashSet<GroupKind> supportedKinds = [];
            if (string.IsNullOrWhiteSpace(raw))
                return supportedKinds;

            string trimmed = raw.Trim().TrimStart('{').TrimEnd('}');
            foreach (string value in trimmed.Split([','], StringSplitOptions.RemoveEmptyEntries)) {
                if (int.TryParse(value.Trim(), out int kindValue) && Enum.IsDefined(typeof(GroupKind), kindValue))
                    supportedKinds.Add((GroupKind)kindValue);
            }

            return supportedKinds;
        }
    }
}