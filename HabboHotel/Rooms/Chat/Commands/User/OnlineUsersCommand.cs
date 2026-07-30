using System.Collections.Generic;
using System.Linq;
using System.Text;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class OnlineUsersCommand : IChatCommand
    {
        public string PermissionRequired => "command_onlineusers";

        public string Parameters => "";

        public string Description => "List everyone currently online.";

        public bool UsableWhileDead => true;

        public bool UsableWhileCuffed => true;

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null)
                return;

            List<string> online = [];
            foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList()) {
                Users.Habbo habbo = client?.GetHabbo();
                if (habbo == null)
                    continue;

                online.Add(habbo.Username);
            }

            StringBuilder message = new();
            message.Append("Online users (<b>" + online.Count + "</b>):\r\r");

            foreach (string username in online.OrderBy(x => x, System.StringComparer.OrdinalIgnoreCase))
                message.Append(username + "\r");

            session.SendNotification(message.ToString());
        }
    }
}