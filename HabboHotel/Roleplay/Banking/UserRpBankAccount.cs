using System.Collections.Generic;

namespace Plus.HabboHotel.Roleplay.Banking
{
    public class UserRpBankAccount
    {
        public int UserId { get; }
        public int Balance { get; private set; }
        public int AccountOpened { get; }

        public bool Dirty { get; private set; }
        public List<UserRpBankLog> PendingLogs { get; }

        public UserRpBankAccount(int userId, int balance, int accountOpened)
        {
            UserId = userId;
            Balance = balance;
            AccountOpened = accountOpened;
            PendingLogs = new List<UserRpBankLog>();
        }

        public void SetBalance(int balance)
        {
            Balance = balance;
            Dirty = true;
        }

        public void AddLog(UserRpBankLog log)
        {
            if (log != null)
                PendingLogs.Add(log);
        }

        public void MarkSaved()
        {
            Dirty = false;
        }
    }
}