using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Banking;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class BankCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_bank";

        public string Parameters => "%username% %type% %amount%";

        public string Description => "Manage bank account";
        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1) {
                session.SendWhisper("Usage :bank <username> <type> <amount>", 1);
                return;
            }

            GameClient target = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);
            if (target == null) {
                session.SendWhisper("Oops, couldn't find that user!", 1);
                return;
            }

            string updateVal = @params[2];
            switch (updateVal.ToLower()) {
                case "open":
                case "create": {
                        bool created = PlusEnvironment.GetBankingManager().CreateAccount(target.GetHabbo());

                        if (!created) {
                            session.SendWhisper("User already has a bank account.", 1);
                            return;
                        }

                        target.SendWhisper("Your bank account has been created.");
                        break;
                    }

                case "withdraw": {
                        if (!session.GetHabbo().GetPermissions().HasCommand("command_give_pixels")) {
                            session.SendWhisper("Oops, it appears that you do not have the permissions to use this command!");
                            break;
                        }

                        if (int.TryParse(@params[3], out int amount)) {
                            BankTransactionResult result = PlusEnvironment.GetBankingManager().Withdraw(target.GetHabbo(), amount, "COMMAND");

                            if (!result.Success) {
                                session.SendWhisper("Withdrawal failed.");
                                return;
                            }

                            session.SendWhisper("You withdrew $" + result.NetAmount + " with a fee of $" + result.FeePaid + ".");
                            return;
                        }

                        session.SendWhisper("Oops, that appears to be an invalid amount!");
                        break;
                    }

                case "deposit": {
                        if (!session.GetHabbo().GetPermissions().HasCommand("command_give_diamonds")) {
                            session.SendWhisper("Oops, it appears that you do not have the permissions to use this command!");
                            break;
                        }

                        if (int.TryParse(@params[3], out int amount)) {
                            BankTransactionResult result = PlusEnvironment.GetBankingManager().Deposit(target.GetHabbo(), amount, "COMMAND");

                            if (!result.Success) {
                                session.SendWhisper("Deposit failed.");
                                return;
                            }

                            session.SendWhisper("Deposited $" + result.NetAmount + " with a fee of $" + result.FeePaid + ".");
                            return;
                        }

                        session.SendWhisper("Oops, that appears to be an invalid amount!");
                        break;
                    }
                default:
                    session.SendWhisper("'" + updateVal + "' is not a valid type! Open/Withdraw/Deposit");
                    break;
            }
        }
    }
}