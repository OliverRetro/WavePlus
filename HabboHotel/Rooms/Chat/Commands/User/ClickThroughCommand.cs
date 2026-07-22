using Plus.Communication.Packets.Outgoing.Roleplay;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class ClickThroughCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_clickthrough";

        public string Parameters => "";

        public string Description => "Toggle click-through so clicking anywhere no longer opens a popup.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            bool clickThrough = session.GetHabbo().ClickThrough;

            session.SendPacket(new GuideSessionPartnerIsPlayingComposer(!clickThrough));

            session.SendWhisper(clickThrough
                ? "Click-through is now disabled."
                : "Click-through is now enabled.", 1);

            session.GetHabbo().ClickThrough = !clickThrough;

            // Persist so the Settings overlay reflects the change (and vice-versa).
            var settings = session.GetHabbo().GetRpSettings();
            if (settings != null) {
                settings.ClickThrough = !clickThrough;
                settings.Save();
            }
        }
    }
}