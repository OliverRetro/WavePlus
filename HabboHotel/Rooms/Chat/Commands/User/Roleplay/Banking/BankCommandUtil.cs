using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Banking
{
    internal static class BankCommandUtil
    {
        public const string NoAccountMessage = "Sorry, you don't have a bank account, visit the bank to open one!";

        public static bool RequireAccount(GameClient session)
        {
            if (session?.GetHabbo()?.GetBankAccount() != null)
                return true;

            session?.SendWhisper(NoAccountMessage, 1);
            return false;
        }

        public static void Shout(GameClient session, Room room, string message)
        {
            Habbo habbo = session?.GetHabbo();
            if (habbo == null || room == null)
                return;

            RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
            if (user == null)
                return;

            room.SendPacket(new ShoutComposer(user.VirtualId, message, 0, 11, isRpAction: true));
        }
    }
}