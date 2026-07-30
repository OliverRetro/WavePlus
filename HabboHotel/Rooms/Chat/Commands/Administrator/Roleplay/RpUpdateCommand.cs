using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Utilities;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay
{
    internal class RpUpdateCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_update";

        public string Parameters => "%switch%";

        public string Description => "Reload RP definitions (" + RpReloadService.Switches + ").";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 2) {
                session.SendWhisper("Usage: :rpupdate <" + RpReloadService.Switches + ">", 1);
                return;
            }

            string result = RpReloadService.Reload(@params[1]);
            session.SendWhisper(result ?? ("Unknown switch. Options: " + RpReloadService.Switches), 1);
        }
    }
}