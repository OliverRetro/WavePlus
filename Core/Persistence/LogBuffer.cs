using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using log4net;
using Plus.Database.EF;
using Plus.Database.EF.Entities;

namespace Plus.Core.Persistence
{
    // Log saver
    public static class LogBuffer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LogBuffer));

        private static readonly ConcurrentQueue<logs_client_staff> CommandLogs = new();
        private static readonly ConcurrentQueue<logs_client_namechange> NameChangeLogs = new();
        private static readonly ConcurrentQueue<logs_client_trade> TradeLogs = new();
        private static readonly ConcurrentQueue<chatlogs_console_invitation> ConsoleInvitations = new();

        public static void LogCommand(int userId, string data, string machineId)
        {
            CommandLogs.Enqueue(new logs_client_staff
            {
                user_id = userId,
                data_string = data,
                machine_id = machineId,
                timestamp = PlusEnvironment.GetUnixTimestamp()
            });
        }

        public static void LogNameChange(int userId, string newName, string oldName)
        {
            NameChangeLogs.Enqueue(new logs_client_namechange
            {
                user_id = userId,
                new_name = newName,
                old_name = oldName,
                timestamp = (int)PlusEnvironment.GetUnixTimestamp()
            });
        }

        public static void LogTrade(int userOneId, int userTwoId, string userOneItems, string userTwoItems)
        {
            TradeLogs.Enqueue(new logs_client_trade
            {
                _1id = userOneId,
                _2id = userTwoId,
                _1items = userOneItems,
                _2items = userTwoItems,
                timestamp = ((long)PlusEnvironment.GetUnixTimestamp()).ToString()
            });
        }

        public static void LogConsoleInvitation(int userId, string message)
        {
            ConsoleInvitations.Enqueue(new chatlogs_console_invitation
            {
                user_id = userId,
                message = message,
                timestamp = PlusEnvironment.GetUnixTimestamp()
            });
        }

        public static void Flush()
        {
            try
            {
                using WavePlusContext db = PlusEnvironment.GetDbContext();
                bool dirty = false;

                dirty |= Drain(CommandLogs, rows => db.logs_client_staffs.AddRange(rows));
                dirty |= Drain(NameChangeLogs, rows => db.logs_client_namechanges.AddRange(rows));
                dirty |= Drain(TradeLogs, rows => db.logs_client_trades.AddRange(rows));
                dirty |= Drain(ConsoleInvitations, rows => db.chatlogs_console_invitations.AddRange(rows));

                if (dirty)
                    db.SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Error($"LogBuffer flush failed: {ex.Message}");
            }
        }

        private static bool Drain<T>(ConcurrentQueue<T> queue, Action<List<T>> addRange)
        {
            if (queue.IsEmpty)
                return false;

            List<T> rows = new();
            while (queue.TryDequeue(out T row))
                rows.Add(row);

            if (rows.Count == 0)
                return false;

            addRange(rows);
            return true;
        }
    }
}
