using Plus.HabboHotel.GameClients;
using System.Globalization;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class BuildHeightCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_buildheight";

        public string Parameters => "%height%";

        public string Description => "Set a specific height for furni.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1) {
                if (session.GetHabbo().CustomBuildHeight != -1) {
                    session.GetHabbo().CustomBuildHeight = -1;
                    session.SendWhisper("Custom Build Height has been disabled.");
                    return;
                } else {
                    session.SendWhisper("Please enter the height. Example usage :bh 2.5!");
                    return;
                }
            }

            string input = @params[1].Replace(',', '.');

            if (!double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double newHeight)) {
                session.SendWhisper("That's not a height. Example usage :bh 2.5!", 1);
                return;
            }

            if (newHeight < 0 || newHeight > 40) {
                session.SendWhisper("The build height must be within 0-40.", 1);
                return;
            }

            session.GetHabbo().CustomBuildHeight = newHeight;
            session.SendWhisper("You've set your new height to: " + newHeight, 1);
            return;
        }
    }
}