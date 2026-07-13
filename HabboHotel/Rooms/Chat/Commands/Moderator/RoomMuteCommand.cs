using System.Collections.Generic;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class RoomMuteCommand : IChatCommand
    {
        public string PermissionRequired => "command_roommute";

        public string Parameters => "%message%";

        public string Description => "Mute the room with a reason.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1)
            {
                session.SendWhisper("Please provide a reason for muting the room to show to the users.");
                return;
            }

            if (!room.RoomMuted)
                room.RoomMuted = true;

            string msg = CommandManager.MergeParams(@params, 1);

            List<RoomUser> roomUsers = room.GetRoomUserManager().GetRoomUsers();
            if (roomUsers.Count > 0)
            {
                foreach (RoomUser user in roomUsers)
                {
                    if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null || user.GetClient().GetHabbo().Username == session.GetHabbo().Username)
                        continue;

                    user.GetClient().SendWhisper("This room has been muted because: " + msg);
                }
            }
        }
    }
}