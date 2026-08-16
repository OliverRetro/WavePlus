using Plus.HabboHotel.GameClients;
using System.Linq;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Roleplay
{
    internal class MassChatCommand : IChatCommand
    {
        public string PermissionRequired => "command_mass_bubble";

        public string Parameters => "%msg%";

        public string Description => "Send a whisper alert to everyone!";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 1) {
                session.SendWhisper("Yeah please let them know what the alert is!");
            }

            var message = string.Join(" ", @params.Skip(1));

            foreach (RoomUser user in PlusEnvironment.GetGame().GetRoomManager().GetAllRoomUsers()) {
                if (user.IsBot)
                    continue;

                GameClient client = user.GetClient();
                if (client == null)
                    continue;

                client.SendWhisper("[STAFF ALERT] " + message, 34);
            }
        }
    }
}