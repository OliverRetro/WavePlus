using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class RoomHealCommand : IChatCommand
    {
        public string PermissionRequired => "command_room_heal";

        public string Parameters => "%amount%";

        public string Description => "Heals the room.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1)
            {
                session.SendWhisper("With global heal, you need to choose the amount to heal, max is 100.");
                return;
            }

            if (!int.TryParse(@params[1], out int amount))
            {
                session.SendWhisper("Please choose an valid number between 0-100.");
                return;
            }

            if (amount > 100 || amount < 0)
            {
                session.SendWhisper("Please choose an valid number between 0-100.");
                return;
            }

            foreach (RoomUser roomUser in room.GetRoomUserManager().GetRoomUsers())
            {
                if (roomUser == null || roomUser.GetClient() == null || session.GetHabbo().Id == roomUser.UserId)
                    continue;

                UserRpStats rpStats = roomUser.GetClient().GetHabbo().GetRpStats();
                if (rpStats.Health < 100) {
                    rpStats.Heal(amount);
                }
            }

            session.SendWhisper("The room has been healed!", 1);
        }
    }
}