using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Corporation
{
    internal class GangChatCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_gang_chat";

        public string Parameters => "%message%";

        public string Description => "Send a message to your gang.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null)
                return;

            if (@params.Length < 2) {
                session.SendWhisper("Please enter the message you want to send to your gang.", 1);
                return;
            }

            Group Gang = session.GetHabbo().GetStats().FavouriteGroupId != 0 && PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(session.GetHabbo().GetStats().FavouriteGroupId, out Group gang) ? gang : null;
            if (Gang == null) {
                session.SendWhisper("You need to have a gang selected.", 1);
                return;
            }

            foreach (RoomUser user in PlusEnvironment.GetGame().GetRoomManager().GetAllRoomUsers()) {
                if (user.IsBot)
                    continue;

                GameClient client = user.GetClient();
                if (client == null || client.GetHabbo() == null)
                    continue;

                if (client.GetHabbo().GetStats().FavouriteGroupId != Gang.Id)
                    continue;

                client.SendWhisper("From " + session.GetHabbo().Username + ": " + CommandManager.MergeParams(@params, 1), 6);
            }
        }
    }
}