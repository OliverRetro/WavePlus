using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Core;
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

        public const int BankCurrencyType = 200;

        public static int ResolveFeePercent(string kind, string managementType)
        {
            string source = string.Equals(managementType, "PHONE", StringComparison.OrdinalIgnoreCase) ? "phone" : "atm";
            var settings = PlusEnvironment.GetSettingsManager();

            string specificKey = $"rp.bank.{kind}.fee.{source}";
            if (settings.ContainsKey(specificKey) && int.TryParse(settings.TryGetValue(specificKey), out int specific))
                return specific;

            return int.TryParse(settings.TryGetValue($"rp.bank.{kind}.fee.default"), out int def) ? def : 0;
        }

        public void SendBankCurrency(Habbo habbo)
        {
            if (habbo?.GetClient() == null)
                return;

            int balance = habbo.GetBankAccount()?.Balance ?? 0;
            habbo.GetClient().SendPacket(new HabboActivityPointNotificationComposer(balance, 0, BankCurrencyType));
        }

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

            int opened = (int)PlusEnvironment.GetUnixTimestamp();

            // The row has to land before the user gets an in-memory account: an account that exists
            // only in memory makes every later balance write a no-op UPDATE, so the user appears to
            // have opened an account, loses the fee, and finds no account again after a relog.
            try {
                using WavePlusContext db = PlusEnvironment.GetDbContext();

                var existing = db.UserRpBanks.Where(b => b.UserId == habbo.Id)
                    .Select(b => new { b.Balance, b.AccountOpened })
                    .FirstOrDefault();

                if (existing != null) {
                    // A row from an earlier attempt is already there — adopt it instead of charging again.
                    UserRpBankAccount recovered = new UserRpBankAccount(habbo.Id, existing.Balance, existing.AccountOpened);
                    recovered.MarkSaved();
                    habbo.SetBankAccount(recovered);
                    SendBankCurrency(habbo);
                    return true;
                }

                db.UserRpBanks.Add(new UserRpBankEntity
                {
                    UserId = habbo.Id,
                    Balance = 0,
                    AccountOpened = opened
                });
                db.SaveChanges();
            } catch (Exception e) {
                ExceptionLogger.LogException(e);
                return false;
            }

            habbo.Credits -= cost;
            habbo.GetClient()?.SendPacket(new CreditBalanceComposer(habbo.Credits));

            UserRpBankAccount account = new UserRpBankAccount(habbo.Id, 0, opened);
            account.MarkSaved();
            habbo.SetBankAccount(account);

            SendBankCurrency(habbo);
            return true;
        }

        public bool DeleteAccount(Habbo habbo)
        {
            if (habbo?.GetBankAccount() == null)
                return false;

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
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
            account.AddLog(new UserRpBankLog(habbo.Id, amount, "DEPOSIT", managementType, fee, (int)PlusEnvironment.GetUnixTimestamp()));
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
            account.AddLog(new UserRpBankLog(habbo.Id, amount, "WITHDRAWAL", managementType, fee, (int)PlusEnvironment.GetUnixTimestamp()));
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

            try {
                using WavePlusContext db = PlusEnvironment.GetDbContext();

                if (account.Dirty) {
                    int balance = account.Balance;
                    int rows = db.UserRpBanks.Where(b => b.UserId == habbo.Id)
                        .ExecuteUpdate(s => s.SetProperty(b => b.Balance, balance));

                    if (rows == 0) {
                        // No row to update — insert one rather than dropping the balance.
                        db.UserRpBanks.Add(new UserRpBankEntity
                        {
                            UserId = habbo.Id,
                            Balance = balance,
                            AccountOpened = account.AccountOpened
                        });
                        db.SaveChanges();
                    }

                    account.MarkSaved();
                }

                // Snapshot first: a transaction landing mid-save must not have its log discarded.
                List<UserRpBankLog> logs = account.PendingLogs.ToList();
                if (logs.Count > 0) {
                    db.UserRpBankLogs.AddRange(logs.Select(log => new Plus.Database.EF.Entities.UserRpBankLogEntity
                    {
                        UserId = log.UserId,
                        Amount = log.Amount,
                        ActionType = log.ActionType,
                        ManagementType = log.ManagementType,
                        FeePaid = log.FeePaid,
                        Timestamp = log.Timestamp
                    }));
                    db.SaveChanges();
                    account.PendingLogs.RemoveRange(0, logs.Count);
                }
            } catch (Exception e) {
                // Left dirty on purpose — the next save cycle retries it.
                ExceptionLogger.LogException(e);
            }
        }

        private static bool CanUseAccount(Habbo habbo, int amount)
        {
            return habbo?.GetBankAccount() != null && amount > 0;
        }
    }
}