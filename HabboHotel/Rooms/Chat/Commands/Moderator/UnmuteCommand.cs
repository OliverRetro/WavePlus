using Plus.Database.Interfaces;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class UnmuteCommand : IChatCommand
    {
        public string PermissionRequired => "command_unmute";

        public string Parameters => "%username%";

        public string Description => "Unmute a currently muted user.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1) {
                session.SendWhisper("Please enter the username of the user you would like to unmute.");
                return;
            }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);
            if (targetClient == null || targetClient.GetHabbo() == null) {
                session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
                return;
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `users` SET `time_muted` = '0' WHERE `id` = '" + targetClient.GetHabbo().Id + "' LIMIT 1");
            }

            targetClient.GetHabbo().TimeMuted = 0;
            targetClient.SendNotification("You have been un-muted by " + session.GetHabbo().Username + "!");
            session.SendWhisper("You have successfully un-muted " + targetClient.GetHabbo().Username + "!");
        }
    }
}