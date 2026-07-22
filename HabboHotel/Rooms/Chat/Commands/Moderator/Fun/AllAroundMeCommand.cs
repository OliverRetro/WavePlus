using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class AllAroundMeCommand : IChatCommand
    {
        public string PermissionRequired => "command_allaroundme";

        public string Parameters => "";

        public string Description => "Need some attention? Pull all of the users to you.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
                return;

            List<RoomUser> users = room.GetRoomUserManager().GetRoomUsers();
            foreach (RoomUser u in users.ToList()) {
                if (u == null || session.GetHabbo().Id == u.UserId)
                    continue;

                u.MoveTo(user.X, user.Y, true);
            }
        }
    }
}