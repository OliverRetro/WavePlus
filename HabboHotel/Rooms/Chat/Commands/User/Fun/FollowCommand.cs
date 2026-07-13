using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    internal class FollowCommand : IChatCommand
    {
        public string PermissionRequired => "command_follow";

        public string Parameters => "%username%";

        public string Description => "Want to visit a specific user? Use this command!";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1)
            {
                session.SendWhisper("Please enter the username of the user you wish to follow.");
                return;
            }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);
            if (targetClient == null)
            {
                session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
                return;
            }

            if (targetClient.GetHabbo().CurrentRoom == session.GetHabbo().CurrentRoom)
            {
                session.SendWhisper("Hey you, open your eyes! " + targetClient.GetHabbo().Username + " is in this room!");
                return;
            }

            if (targetClient.GetHabbo().Username == session.GetHabbo().Username)
            {
                session.SendWhisper("Sadooooooooo!");
                return;
            }

            if (!targetClient.GetHabbo().InRoom)
            {
                session.SendWhisper("That user currently isn't in a room!");
                return;
            }

            if (targetClient.GetHabbo().CurrentRoom.Access != RoomAccess.Open && !session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                session.SendWhisper("Oops, the room that user is either locked, passworded or invisible. You cannot follow!");
                return;
            }

            session.GetHabbo().PrepareRoom(targetClient.GetHabbo().CurrentRoom.RoomId, "");
        }
    }
}