using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class LayCommand : IChatCommand
    {
        public string PermissionRequired => "command_lay";

        public string Parameters => "";

        public string Description => "Allows you to lay down in the room, without needing a bed.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
                return;

            if (!room.GetGameMap().ValidTile(user.X + 2, user.Y + 2) && !room.GetGameMap().ValidTile(user.X + 1, user.Y + 1))
            {
                session.SendWhisper("Oops, cannot lay down here - try elsewhere!", 1);
                return;
            }

            if (user.HasStatus("sit") || user.IsSitting || user.RidingHorse || user.IsWalking)
                return;

            user.Lay();
        }
    }
}