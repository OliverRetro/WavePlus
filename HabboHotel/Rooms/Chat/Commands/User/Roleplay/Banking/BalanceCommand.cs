using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Banking;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Banking
{
    internal class BalanceCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_bank_balance";

        public string Parameters => "";

        public string Description => "Check your bank account balance.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room == null)
                return;

            if (!BankCommandUtil.RequireAccount(session))
                return;

            UserRpBankAccount account = session.GetHabbo().GetBankAccount();
            BankCommandUtil.Shout(session, room,
                "*checks their account balance and notices they have $" + account.Balance + "*");
        }
    }
}