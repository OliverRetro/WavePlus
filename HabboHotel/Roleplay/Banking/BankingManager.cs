using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Users;
using System;
using System.Data;

namespace Plus.HabboHotel.Roleplay.Banking
{
    public class BankingManager
    {
        public UserRpBankAccount LoadAccount(int userId)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `user_id`,`balance`,`account_opened` FROM `user_rp_bank` WHERE `user_id` = @user_id LIMIT 1");
                dbClient.AddParameter("user_id", userId);
                DataRow row = dbClient.GetRow();
                if (row == null)
                    return null;

                return new UserRpBankAccount(
                    Convert.ToInt32(row["user_id"]),
                    Convert.ToInt32(row["balance"]),
                    Convert.ToDateTime(row["account_opened"]));
            }
        }

        public bool CreateAccount(Habbo habbo)
        {
            if (habbo == null || habbo.GetBankAccount() != null)
                return false;

            int cost = int.TryParse(PlusEnvironment.GetSettingsManager().TryGetValue("rp.bank.account.cost"), out int amount) ? amount : 50;
            if (habbo.Credits < cost)
                return false;

            habbo.Credits -= cost;
            habbo.GetClient()?.SendPacket(new CreditBalanceComposer(habbo.Credits));

            UserRpBankAccount account = new UserRpBankAccount(habbo.Id, 0, DateTime.UtcNow);
            account.MarkSaved();
            habbo.SetBankAccount(account);

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `user_rp_bank` (`user_id`, `balance`, `account_opened`) VALUES (@user_id, @balance, @account_opened)");
                dbClient.AddParameter("user_id", habbo.Id);
                dbClient.AddParameter("balance", 0);
                dbClient.AddParameter("account_opened", account.AccountOpened);
                dbClient.RunQuery();
            }

            return true;
        }

        public bool DeleteAccount(Habbo habbo)
        {
            if (habbo?.GetBankAccount() == null)
                return false;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("DELETE FROM `user_rp_bank_logs` WHERE `user_id` = @user_id");
                dbClient.AddParameter("user_id", habbo.Id);
                dbClient.RunQuery();

                dbClient.SetQuery("DELETE FROM `user_rp_bank` WHERE `user_id` = @user_id LIMIT 1");
                dbClient.AddParameter("user_id", habbo.Id);
                dbClient.RunQuery();
            }

            habbo.SetBankAccount(null);
            return true;
        }

        public BankTransactionResult Deposit(Habbo habbo, int amount, string managementType)
        {
            if (!CanUseAccount(habbo, amount) || habbo.Credits < amount)
                return new BankTransactionResult(false, 0, 0);

            int feePercentage = int.TryParse(
                PlusEnvironment.GetSettingsManager().TryGetValue("rp.bank.deposit.fee.default"),
                out int parsedFeePercentage)
                    ? parsedFeePercentage
                    : 0;

            int fee = amount * feePercentage / 100;
            int netAmount = amount - fee;

            if (netAmount < 0)
                return new BankTransactionResult(false, 0, 0);

            habbo.Credits -= amount;
            habbo.GetClient()?.SendPacket(new CreditBalanceComposer(habbo.Credits));

            UserRpBankAccount account = habbo.GetBankAccount();
            account.SetBalance(account.Balance + netAmount);
            account.AddLog(new UserRpBankLog(habbo.Id, amount, "DEPOSIT", managementType, fee, DateTime.UtcNow));

            return new BankTransactionResult(true, fee, netAmount);
        }

        public BankTransactionResult Withdraw(Habbo habbo, int amount, string managementType)
        {
            if (!CanUseAccount(habbo, amount))
                return new BankTransactionResult(false, 0, 0);

            UserRpBankAccount account = habbo.GetBankAccount();
            if (account.Balance < amount)
                return new BankTransactionResult(false, 0, 0);

            int feePercentage = int.TryParse(
                PlusEnvironment.GetSettingsManager().TryGetValue("rp.bank.withdrawal.fee.default"),
                out int parsedFeePercentage)
                    ? parsedFeePercentage
                    : 0;

            int fee = amount * feePercentage / 100;
            int netAmount = amount - fee;

            if (netAmount < 0)
                return new BankTransactionResult(false, 0, 0);

            account.SetBalance(account.Balance - amount);
            account.AddLog(new UserRpBankLog(habbo.Id, amount, "WITHDRAWAL", managementType, fee, DateTime.UtcNow));

            habbo.Credits += netAmount;
            habbo.GetClient()?.SendPacket(new CreditBalanceComposer(habbo.Credits));

            return new BankTransactionResult(true, fee, netAmount);
        }

        public void Save(Habbo habbo)
        {
            UserRpBankAccount account = habbo?.GetBankAccount();
            if (account == null)
                return;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                if (account.Dirty)
                {
                    dbClient.SetQuery("UPDATE `user_rp_bank` SET `balance` = @balance WHERE `user_id` = @user_id LIMIT 1");
                    dbClient.AddParameter("balance", account.Balance);
                    dbClient.AddParameter("user_id", habbo.Id);
                    dbClient.RunQuery();
                    account.MarkSaved();
                }

                foreach (UserRpBankLog log in account.PendingLogs)
                {
                    dbClient.SetQuery("INSERT INTO `user_rp_bank_logs` (`user_id`, `amount`, `action_type`, `management_type`, `fee_paid`, `timestamp`) VALUES (@user_id, @amount, @action_type, @management_type, @fee_paid, @timestamp)");
                    dbClient.AddParameter("user_id", log.UserId);
                    dbClient.AddParameter("amount", log.Amount);
                    dbClient.AddParameter("action_type", log.ActionType);
                    dbClient.AddParameter("management_type", log.ManagementType);
                    dbClient.AddParameter("fee_paid", log.FeePaid);
                    dbClient.AddParameter("timestamp", log.Timestamp);
                    dbClient.RunQuery();
                }

                account.PendingLogs.Clear();
            }
        }

        private static bool CanUseAccount(Habbo habbo, int amount)
        {
            return habbo?.GetBankAccount() != null && amount > 0;
        }

        private static int CalculateFee(int amount, int percent)
        {
            return percent <= 0 ? 0 : (int)Math.Ceiling(amount * (percent / 100.0));
        }
    }
}
