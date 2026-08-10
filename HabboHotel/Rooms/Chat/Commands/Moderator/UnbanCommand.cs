using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class UnbanCommand : IChatCommand
    {
        public string PermissionRequired => "command_unban";

        public string Parameters => "%username%";

        public string Description => "Immediately lift every ban (account, IP and machine) on a user.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1) {
                session.SendWhisper("Please enter the username of the user you'd like to unban.");
                return;
            }

            Habbo habbo = PlusEnvironment.GetHabboByUsername(@params[1]);
            if (habbo == null) {
                session.SendWhisper("An error occoured whilst finding that user in the database.");
                return;
            }

            PlusEnvironment.GetGame().GetModerationManager().Unban(habbo.Id);

            session.SendWhisper("Success, you have unbanned the user '" + habbo.Username + "'. They may now connect again.");
        }
    }
}