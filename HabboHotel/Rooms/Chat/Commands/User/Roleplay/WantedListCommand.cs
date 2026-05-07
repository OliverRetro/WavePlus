using Plus.Communication.Packets.Outgoing.Roleplay;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class WantedListCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_wanted_list";

        public string Parameters => "";

        public string Description => "Get the wanted list.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            session.SendPacket(new WantedListComposer());
        }
    }
}