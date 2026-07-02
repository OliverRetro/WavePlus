using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Utilities;
using System;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator
{
    internal class CarryCommand : IChatCommand
    {
        public string PermissionRequired => "command_carry";

        public string Parameters => "%ItemId%";

        public string Description => "Allows you to carry a hand item";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (!int.TryParse(Convert.ToString(@params[1]), out int itemId)) {
                session.SendWhisper("Please enter a valid integer.");
                return;
            }

            RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            user?.CarryItem(itemId);
        }
    }
}