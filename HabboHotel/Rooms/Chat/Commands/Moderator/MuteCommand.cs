using Plus.Database.Interfaces;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class MuteCommand : IChatCommand
    {
        public string PermissionRequired => "command_mute";

        public string Parameters => "%username% %time%";

        public string Description => "Mute another user for a certain amount of time.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1) {
                session.SendWhisper("Please enter a username and a valid time in seconds (max 600, anything over will be set back to 600).");
                return;
            }

            Habbo habbo = PlusEnvironment.GetHabboByUsername(@params[1]);
            if (habbo == null) {
                session.SendWhisper("An error occoured whilst finding that user in the database.");
                return;
            }

            if (habbo.GetPermissions().HasRight("mod_tool") && !session.GetHabbo().GetPermissions().HasRight("mod_mute_any")) {
                session.SendWhisper("Oops, you cannot mute that user.");
                return;
            }

            if (double.TryParse(@params[2], out double time)) {
                if (time > 600 && !session.GetHabbo().GetPermissions().HasRight("mod_mute_limit_override"))
                    time = 600;

                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `users` SET `time_muted` = '" + time + "' WHERE `id` = '" + habbo.Id + "' LIMIT 1");
                }

                if (habbo.GetClient() != null) {
                    habbo.TimeMuted = time;
                    habbo.GetClient().SendNotification("You have been muted by a moderator for " + time + " seconds!");
                }

                session.SendWhisper("You have successfully muted " + habbo.Username + " for " + time + " seconds.");
            } else
                session.SendWhisper("Please enter a valid integer.");
        }
    }
}