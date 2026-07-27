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

        public string Description => "List everyone online and the room they're in.";

        public bool UsableWhileDead => true;

        public bool UsableWhileCuffed => true;

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null)
                return;

            List<(string Username, string Room)> online = new();
            foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList()) {
                Users.Habbo habbo = client?.GetHabbo();
                if (habbo == null)
                    continue;

                string roomName = habbo.CurrentRoom?.Name;
                online.Add((habbo.Username, string.IsNullOrWhiteSpace(roomName) ? "In the hotel" : roomName));
            }

            StringBuilder message = new();
            message.Append("Online users (" + online.Count + "):\r\r");

            foreach ((string username, string roomName) in online.OrderBy(x => x.Username, System.StringComparer.OrdinalIgnoreCase))
                message.Append(username + " - " + roomName + "\r");

            session.SendNotification(message.ToString());
        }
    }
}
