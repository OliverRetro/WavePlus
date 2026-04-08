using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class RoomHealCommand : IChatCommand
    {
        public string PermissionRequired => "command_room_heal";

        public string Parameters => "";

        public string Description => "Heals the room.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            foreach (RoomUser roomUser in room.GetRoomUserManager().GetRoomUsers())
            {
                if (roomUser == null || roomUser.GetClient() == null || session.GetHabbo().Id == roomUser.UserId)
                    continue;

                UserRpStats rpStats = roomUser.GetClient().GetHabbo().GetRpStats();
                if (rpStats.Health < 100) {
                    rpStats.Heal(100);
                }
            }

            session.SendWhisper("The room has been healed!", 1);
        }
    }
}