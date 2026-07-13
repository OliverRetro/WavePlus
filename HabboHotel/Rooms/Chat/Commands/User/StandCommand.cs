using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class StandCommand : IChatCommand
    {
        public string PermissionRequired => "command_stand";

        public string Parameters => "";

        public string Description => "Allows you to stand up if not stood already.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Username);
            if (user == null)
                return;

            if (user.IsSitting)
            {
                user.Statusses.Remove("sit");
                user.Z += 0.35;
                user.IsSitting = false;
                user.UpdateNeeded = true;
            }
            else if (user.IsLying)
            {
                user.Statusses.Remove("lay");
                user.Z += 0.35;
                user.IsLying = false;
                user.UpdateNeeded = true;
            }
        }
    }
}