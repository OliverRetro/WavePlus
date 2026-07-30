using System.Collections.Generic;

namespace Plus.HabboHotel.Users.Permissions
{
    public sealed class PermissionComponent
    {
        private readonly List<string> _permissions;

        private readonly List<string> _commands;

        public PermissionComponent()
        {
            _permissions = new List<string>();
            _commands = new List<string>();
        }

        public bool Init(Habbo habbo)
        {
            if (_permissions.Count > 0)
                _permissions.Clear();

            if (_commands.Count > 0)
                _commands.Clear();

            _permissions.AddRange(PlusEnvironment.GetGame().GetPermissionManager().GetPermissionsForPlayer(habbo));
            _commands.AddRange(PlusEnvironment.GetGame().GetPermissionManager().GetCommandsForPlayer(habbo));
            return true;
        }

        public bool HasRight(string right)
        {
            return _permissions.Contains(right);
        }

        public bool HasCommand(string command)
        {
            return _commands.Contains(command);
        }

        public void Dispose()
        {
            _permissions.Clear();
        }
    }
}