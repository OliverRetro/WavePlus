using System;
using System.Linq;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.RpItem;
using Plus.HabboHotel.Roleplay.RpItem.Item;
using Plus.HabboHotel.Roleplay.Utilities;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay
{
    internal class LiveFeedTestCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_livefeed_test";

        public string Parameters => "%input%";

        public string Description => "Send a full livefeed test.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 2)
            {
                session.SendWhisper("Please actually send something.", 1);
                return;
            }

            LiveFeedService.LiveFeed(@params[1]);
        }
    }
}
