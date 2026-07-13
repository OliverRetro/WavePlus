using System.Linq;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class MassBadgeCommand : IChatCommand
    {
        public string PermissionRequired => "command_mass_badge";

        public string Parameters => "%badge%";

        public string Description => "Give a badge to the entire hotel.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1)
            {
                session.SendWhisper("Please enter the code of the badge you'd like to give to the entire hotel.");
                return;
            }

            foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
            {
                if (client == null || client.GetHabbo() == null || client.GetHabbo().Username == session.GetHabbo().Username)
                    continue;

                if (!client.GetHabbo().GetBadgeComponent().HasBadge(@params[1]))
                {
                    client.GetHabbo().GetBadgeComponent().GiveBadge(@params[1], true, client);
                    client.SendNotification("You have just been given a badge!");
                }
                else
                    client.SendWhisper(session.GetHabbo().Username + " tried to give you a badge, but you already have it!");
            }

            session.SendWhisper("You have successfully given every user in this hotel the " + @params[1] + " badge!");
        }
    }
}