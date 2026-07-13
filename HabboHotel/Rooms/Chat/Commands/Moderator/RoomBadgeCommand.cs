using System.Linq;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class RoomBadgeCommand : IChatCommand
    {
        public string PermissionRequired => "command_room_badge";

        public string Parameters => "%badge%";

        public string Description => "Give a badge to the entire room!";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1)
            {
                session.SendWhisper("Please enter the name of the badge you'd like to give to the room.");
                return;
            }

            foreach (RoomUser user in room.GetRoomUserManager().GetUserList().ToList())
            {
                if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                    continue;

                if (!user.GetClient().GetHabbo().GetBadgeComponent().HasBadge(@params[1]))
                {
                    user.GetClient().GetHabbo().GetBadgeComponent().GiveBadge(@params[1], true, user.GetClient());
                    user.GetClient().SendNotification("You have just been given a badge!");
                }
                else
                    user.GetClient().SendWhisper(session.GetHabbo().Username + " tried to give you a badge, but you already have it!");
            }

            session.SendWhisper("You have successfully given every user in this room the " + @params[2] + " badge!");
        }
    }
}