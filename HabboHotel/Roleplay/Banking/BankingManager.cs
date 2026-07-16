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
        /// Activity-point currency type the bank balance is published under, so the client purse
        /// (and the phone/ATM "Current Balance") can display it. Keep in sync with the Nitro client
        /// (PhoneBankApp BANK_CURRENCY_ID / ATMView).
        /// </summary>
        public const int BankCurrencyType = 200;

        /// <summary>
        /// Resolves the fee percentage for a transaction. <paramref name="kind"/> is "deposit" or
        /// "withdrawal"; <paramref name="managementType"/> is the source ("ATM" vs "PHONE"). Prefers the
        /// source-specific setting (rp.bank.{kind}.fee.{atm|phone}) and falls back to rp.bank.{kind}.fee.default.
        /// </summary>
        public static int ResolveFeePercent(string kind, string managementType)
        {
            string source = string.Equals(managementType, "PHONE", StringComparison.OrdinalIgnoreCase) ? "phone" : "atm";
            var settings = PlusEnvironment.GetSettingsManager();

            string specificKey = $"rp.bank.{kind}.fee.{source}";
            if (settings.ContainsKey(specificKey) && int.TryParse(settings.TryGetValue(specificKey), out int specific))
                return specific;

            return int.TryParse(settings.TryGetValue($"rp.bank.{kind}.fee.default"), out int def) ? def : 0;
        }

        /// <summary>Publishes the user's bank balance to the client purse as currency type 200.</summary>
        public void SendBankCurrency(Habbo habbo)
        {
            if (habbo?.GetClient() == null)
                return;

            int balance = habbo.GetBankAccount()?.Balance ?? 0;
            habbo.GetClient().SendPacket(new HabboActivityPointNotificationComposer(balance, 0, BankCurrencyType));
        }

        /// <summary>
        /// Sends both the ATM (6006) and Phone (6007) bank snapshots to a user.
        /// </summary>
        public void SendBankSnapshot(Habbo habbo)
        {
            if (habbo?.GetClient() == null)
                return;

            // NOTE: the legacy AtmBankComposer (header 6006) collides with the Nitro client's
            // GetGroupsRolesListEvent, so it is intentionally NOT sent — the phone/ATM read the
            // balance from the purse (currency 200) instead, published below.
            SendBankCurrency(habbo);
        }

        public List<UserRpBankLog> LoadRecentLogs(int userId)
        {
            using WavePlusContext db = PlusEnvironment.GetDbContext();
            return db.UserRpBankLogs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Timestamp)
                .Take(RecentLogLimit)
                .Select(l => new { l.UserId, l.Amount, l.ActionType, l.ManagementType, l.FeePaid, l.Timestamp })
                .ToList()
                .Select(l => new UserRpBankLog(l.UserId, l.Amount, l.ActionType, l.ManagementType, l.FeePaid, l.Timestamp))
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
            var row = db.UserRpBanks.Where(b => b.UserId == userId)
                .Select(b => new { b.UserId, b.Balance, b.AccountOpened })
                .FirstOrDefault();
            if (row == null)
                return null;

            return new UserRpBankAccount(row.UserId, row.Balance, row.AccountOpened);
        }

        public bool CreateAccount(Habbo habbo)
        {
            if (habbo == null || habbo.GetBankAccount() != null)
                return false;

            int cost = int.TryParse(PlusEnvironment.GetSettingsManager().TryGetValue("rp.bank.account.opening.cost"), out int amount) ? amount : 50;
            if (habbo.Credits < cost)
                return false;

            habbo.Credits -= cost;
            habbo.GetClient()?.SendPacket(new CreditBalanceComposer(habbo.Credits));

            UserRpBankAccount account = new UserRpBankAccount(habbo.Id, 0, DateTime.UtcNow);
            account.MarkSaved();
            habbo.SetBankAccount(account);

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            db.UserRpBanks.Add(new UserRpBankEntity
            {
                UserId = habbo.Id,
                Balance = 0,
                AccountOpened = account.AccountOpened
            });
            db.SaveChanges();

            SendBankCurrency(habbo);
            return true;
        }

        public bool DeleteAccount(Habbo habbo)
        {
            if (habbo?.GetBankAccount() == null)
                return false;

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                db.UserRpBankLogs.Where(l => l.UserId == habbo.Id).ExecuteDelete();
                db.UserRpBanks.Where(b => b.UserId == habbo.Id).ExecuteDelete();
            }

            habbo.SetBankAccount(null);
            return true;
        }

        public BankTransactionResult Deposit(Habbo habbo, int amount, string managementType)
        {
            if (!CanUseAccount(habbo, amount) || habbo.Credits < amount)
                return new BankTransactionResult(false, 0, 0);

            int feePercentage = ResolveFeePercent("deposit", managementType);

            int fee = amount * feePercentage / 100;
            int netAmount = amount - fee;

            if (netAmount < 0)
                return new BankTransactionResult(false, 0, 0);

            habbo.Credits -= amount;
            habbo.GetClient()?.SendPacket(new CreditBalanceComposer(habbo.Credits));

            UserRpBankAccount account = habbo.GetBankAccount();
            account.SetBalance(account.Balance + netAmount);
            account.AddLog(new UserRpBankLog(habbo.Id, amount, "DEPOSIT", managementType, fee, DateTime.UtcNow));
            SendBankCurrency(habbo);

            return new BankTransactionResult(true, fee, netAmount);
        }

        public BankTransactionResult Withdraw(Habbo habbo, int amount, string managementType)
        {
            if (!CanUseAccount(habbo, amount))
                return new BankTransactionResult(false, 0, 0);

            UserRpBankAccount account = habbo.GetBankAccount();
            if (account.Balance < amount)
                return new BankTransactionResult(false, 0, 0);

            int feePercentage = ResolveFeePercent("withdrawal", managementType);

            int fee = amount * feePercentage / 100;
            int netAmount = amount - fee;

            if (netAmount < 0)
                return new BankTransactionResult(false, 0, 0);

            account.SetBalance(account.Balance - amount);
            account.AddLog(new UserRpBankLog(habbo.Id, amount, "WITHDRAWAL", managementType, fee, DateTime.UtcNow));
            SendBankCurrency(habbo);

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

            if (account.Dirty)
            {
                db.UserRpBanks.Where(b => b.UserId == habbo.Id)
                    .ExecuteUpdate(s => s.SetProperty(b => b.Balance, account.Balance));
                account.MarkSaved();
            }

            if (account.PendingLogs.Count > 0)
            {
                db.UserRpBankLogs.AddRange(account.PendingLogs.Select(log => new Plus.Database.EF.Entities.UserRpBankLogEntity
                {
                    UserId = log.UserId,
                    Amount = log.Amount,
                    ActionType = log.ActionType,
                    ManagementType = log.ManagementType,
                    FeePaid = log.FeePaid,
                    Timestamp = log.Timestamp
                }));
                db.SaveChanges();
                account.PendingLogs.Clear();
            }
        }

        private static bool CanUseAccount(Habbo habbo, int amount)
        {
            return habbo?.GetBankAccount() != null && amount > 0;
        }
    }
}
