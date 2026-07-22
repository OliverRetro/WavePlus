using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Banking;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Banking
{
    internal class WithdrawCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_bank_withdraw";

        public string Parameters => "%amount%";

        public string Description => "Withdraw money from your bank account to your wallet (5% ATM fee).";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room == null)
                return;

            if (!BankCommandUtil.RequireAccount(session))
                return;

            if (@params.Length < 2 || !int.TryParse(@params[1], out int amount) || amount <= 0) {
                session.SendWhisper("Please enter a valid amount to withdraw.", 1);
                return;
            }

            UserRpBankAccount account = session.GetHabbo().GetBankAccount();
            if (account.Balance < amount) {
                session.SendWhisper("You don't have this amount to withdraw!", 1);
                return;
            }

            string source = (@params.Length >= 3 && @params[2].Equals("phone", StringComparison.OrdinalIgnoreCase)) ? "PHONE" : "ATM";

            BankTransactionResult result = PlusEnvironment.GetBankingManager().Withdraw(session.GetHabbo(), amount, source);
            if (!result.Success) {
                session.SendWhisper("You don't have this amount to withdraw!", 1);
                return;
            }

            BankCommandUtil.Shout(session, room,
                "*withdraws $" + result.NetAmount + " from their bank account*");

            PlusEnvironment.GetBankingManager().SendBankSnapshot(session.GetHabbo());
        }
    }
}