using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class FreezeCommand : IChatCommand
    {
        public string PermissionRequired => "command_freeze";

        public string Parameters => "%username%";

        public string Description => "Prevent another user from walking.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1)
            {
                session.SendWhisper("Please enter the username of the user you wish to freeze.");
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
                targetUser.Frozen = true;

            session.SendWhisper("Successfully froze " + targetClient.GetHabbo().Username + "!");
        }
    }
}