using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class AlertCommand : IChatCommand
    {
        public string PermissionRequired => "command_alert_user";

        public string Parameters => "%username% %Messages%";

        public string Description => "Alert a user with a message of your choice.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1)
            {
                session.SendWhisper("Please enter the username of the user you wish to alert.");
                return;
            }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);
            if (targetClient == null)
            {
                session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
                return;
            }

            if (targetClient.GetHabbo() == null)
            {
                session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
                return;
            }

            if (targetClient.GetHabbo().Username == session.GetHabbo().Username)
            {
                session.SendWhisper("Get a life.");
                return;
            }

            string message = CommandManager.MergeParams(@params, 2);

            targetClient.SendNotification(session.GetHabbo().Username + " alerted you with the following message:\n\n" + message);
            session.SendWhisper("Alert successfully sent to " + targetClient.GetHabbo().Username);
        }
    }
}