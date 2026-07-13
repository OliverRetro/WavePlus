using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class SetSpeedCommand : IChatCommand
    {
        public string PermissionRequired => "command_setspeed";

        public string Parameters => "%value%";

        public string Description => "Set the speed of the rollers in the current room.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (!room.CheckRights(session, true))
                return;

            if (@params.Length == 1)
            {
                session.SendWhisper("Please enter a value for the roller speed.");
                return;
            }

            if (int.TryParse(@params[1], out int speed))
            {
                session.GetHabbo().CurrentRoom.GetRoomItemHandler().SetSpeed(speed);
            }
            else
                session.SendWhisper("Invalid amount, please enter a valid number.");
        }
    }
}