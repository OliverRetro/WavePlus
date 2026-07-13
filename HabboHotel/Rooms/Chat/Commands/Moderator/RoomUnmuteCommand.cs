using System.Collections.Generic;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class RoomUnmuteCommand : IChatCommand
    {
        public string PermissionRequired => "command_unroommute";

        public string Parameters => "";

        public string Description => "Unmute the room.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (!room.RoomMuted)
            {
                session.SendWhisper("This room isn't muted.");
                return;
            }

            room.RoomMuted = false;

            List<RoomUser> roomUsers = room.GetRoomUserManager().GetRoomUsers();
            if (roomUsers.Count > 0)
            {
                foreach (RoomUser user in roomUsers)
                {
                    if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null || user.GetClient().GetHabbo().Username == session.GetHabbo().Username)
                        continue;

                    user.GetClient().SendWhisper("This room has been un-muted .");
                }
            }
        }
    }
}