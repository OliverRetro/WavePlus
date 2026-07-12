using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Roleplay;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Plus.HabboHotel.Roleplay.Banking
{
    public class BankingManager
    {
        private const int RecentLogLimit = 25;

        /// <summary>
        /// Sends both the ATM (6006) and Phone (6007) bank snapshots to a user.
        /// </summary>
        public void SendBankSnapshot(Habbo habbo)
        {
            if (habbo?.GetClient() == null)
                return;

            UserRpBankAccount account = habbo.GetBankAccount();
            bool hasAccount = account != null;
            int bankBalance = account?.Balance ?? 0;
            int walletBalance = habbo.Credits;
            string accountNumber = hasAccount ? habbo.Id.ToString("D8") : string.Empty;
            double feePercentage = GetDepositFeeFraction();

            habbo.GetClient().SendPacket(new AtmBankComposer(hasAccount, accountNumber, bankBalance, walletBalance, feePercentage, hasAccount, hasAccount));
            habbo.GetClient().SendPacket(new PhoneBankComposer(hasAccount, accountNumber, bankBalance, walletBalance, feePercentage, hasAccount, hasAccount, hasAccount ? LoadRecentLogs(habbo.Id) : new List<UserRpBankLog>()));
        }

        public List<UserRpBankLog> LoadRecentLogs(int userId)
        {
            using WavePlusContext db = PlusEnvironment.GetDbContext();
            return db.user_rp_bank_logs
                .Where(l => l.user_id == userId)
                .OrderByDescending(l => l.timestamp)
                .Take(RecentLogLimit)
                .Select(l => new { l.user_id, l.amount, l.action_type, l.management_type, l.fee_paid, l.timestamp })
                .ToList()
                .Select(l => new UserRpBankLog(l.user_id, l.amount, l.action_type, l.management_type, l.fee_paid, l.timestamp))
                .ToList();
        }

        private static double GetDepositFeeFraction()
        {
            int percent = int.TryParse(PlusEnvironment.GetSettingsManager().TryGetValue("rp.bank.deposit.fee.default"), out int p) ? p : 0;
            return percent / 100.0;
        }

        public UserRpBankAccount LoadAccount(int userId)
        {
            using WavePlusContext db = PlusEnvironment.GetDbContext();
            var row = db.user_rp_banks.Where(b => b.user_id == userId)
                .Select(b => new { b.user_id, b.balance, b.account_opened })
                .FirstOrDefault();
            if (row == null)
                return null;

            return new UserRpBankAccount(row.user_id, row.balance, row.account_opened);
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

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            db.user_rp_banks.Add(new user_rp_bank
            {
                user_id = habbo.Id,
                balance = 0,
                account_opened = account.AccountOpened
            });
            db.SaveChanges();

            return true;
        }

        public bool DeleteAccount(Habbo habbo)
        {
            if (habbo?.GetBankAccount() == null)
                return false;

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                db.user_rp_bank_logs.Where(l => l.user_id == habbo.Id).ExecuteDelete();
                db.user_rp_banks.Where(b => b.user_id == habbo.Id).ExecuteDelete();
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

            using WavePlusContext db = PlusEnvironment.GetDbContext();

            if (account.Dirty) {
                db.user_rp_banks.Where(b => b.user_id == habbo.Id)
                    .ExecuteUpdate(s => s.SetProperty(b => b.balance, account.Balance));
                account.MarkSaved();
            }

            if (account.PendingLogs.Count > 0) {
                db.user_rp_bank_logs.AddRange(account.PendingLogs.Select(log => new user_rp_bank_log
                {
                    user_id = log.UserId,
                    amount = log.Amount,
                    action_type = log.ActionType,
                    management_type = log.ManagementType,
                    fee_paid = log.FeePaid,
                    timestamp = log.Timestamp
                }));
                db.SaveChanges();
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
