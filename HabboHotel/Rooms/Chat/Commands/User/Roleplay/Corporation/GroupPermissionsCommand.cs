using System.Linq;
using System.Text;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Corporation
{
    internal class GroupPermissionsCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_group_permissions";

        public string Parameters => "";

        public string Description => "View your current room group's role and permissions.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room?.Group == null)
                return;

            Group group = room.Group;
            int userId = session.GetHabbo().Id;
            if (!group.IsMember(userId)) {
                session.SendWhisper("You are not a member of this group.", 1);
                return;
            }

            int level = group.GetMemberLevel(userId);
            StringBuilder info = new();
            info.Append("<b>Group Permissions</b>\r");
            info.Append("Group: <b>" + group.Name + "</b>\r");
            info.Append("Role: <b>" + group.GetMemberRoleName(userId) + "</b>\r");
            info.Append("Level: <b>" + level + "</b>\r\r");

            var permissions = group.GetActivePermissionsForUser(userId).ToList();
            if (permissions.Count == 0) {
                info.Append("Permissions: none\r");
            } else {
                info.Append("<b>Permissions:</b>\r");
                foreach (GroupPermissionKey permission in permissions)
                    info.Append(permission.Name + " (" + permission.Key + ")\r");
            }

            session.SendNotification(info.ToString());
        }
    }
}