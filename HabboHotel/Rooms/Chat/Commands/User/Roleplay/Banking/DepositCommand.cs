using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Banking;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Banking
{
    internal class DepositCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_bank_deposit";

        public string Parameters => "%amount%";

        public string Description => "Deposit credits from your wallet into your bank account.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room == null)
                return;

            if (!BankCommandUtil.RequireAccount(session))
                return;

            if (@params.Length < 2 || !int.TryParse(@params[1], out int amount) || amount <= 0) {
                session.SendWhisper("Please enter a valid amount to deposit.", 1);
                return;
            }

            if (session.GetHabbo().Credits < amount) {
                session.SendWhisper("You don't have this amount to deposit!", 1);
                return;
            }

            string source = (@params.Length >= 3 && @params[2].Equals("phone", StringComparison.OrdinalIgnoreCase)) ? "PHONE" : "ATM";

            BankTransactionResult result = PlusEnvironment.GetBankingManager().Deposit(session.GetHabbo(), amount, source);
            if (!result.Success) {
                session.SendWhisper("You don't have this amount to deposit!", 1);
                return;
            }

            BankCommandUtil.Shout(session, room,
                "*deposits $" + result.NetAmount + " into their bank account*");

            // Refresh the ATM/phone snapshot so the open view reflects the new balances.
            PlusEnvironment.GetBankingManager().SendBankSnapshot(session.GetHabbo());
        }
    }
}