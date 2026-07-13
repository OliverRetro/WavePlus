using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class HotelAlertCommand : IChatCommand
    {
        public string PermissionRequired => "command_hotel_alert";

        public string Parameters => "%message%";

        public string Description => "Send a message to the entire hotel.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1)
            {
                session.SendWhisper("Please enter a message to send.");
                return;
            }

            string message = CommandManager.MergeParams(@params, 1);
            PlusEnvironment.GetGame().GetClientManager().SendPacket(new BroadcastMessageAlertComposer(message + "\r\n" + "- " + session.GetHabbo().Username));
        }
    }
}