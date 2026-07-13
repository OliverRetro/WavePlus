using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class UnFreezeCommand : IChatCommand
    {
        public string PermissionRequired => "command_unfreeze";

        public string Parameters => "%username%";

        public string Description => "Allow another user to walk again.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1)
            {
                session.SendWhisper("Please enter the username of the user you wish to un-freeze.");
                return;
            }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);
            if (targetClient == null)
            {
                session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
                return;
            }

            RoomUser targetUser = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(@params[1]);
            if (targetUser != null)
                targetUser.Frozen = false;

            session.SendWhisper("Successfully unfroze " + targetClient.GetHabbo().Username + "!");
        }
    }
}