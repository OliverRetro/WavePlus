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

        private static readonly ConcurrentQueue<LogsClientStaffEntity> CommandLogs = new();
        private static readonly ConcurrentQueue<LogsClientNamechangeEntity> NameChangeLogs = new();
        private static readonly ConcurrentQueue<LogsClientTradeEntity> TradeLogs = new();
        private static readonly ConcurrentQueue<ChatlogsConsoleInvitationEntity> ConsoleInvitations = new();

        public static void LogCommand(int userId, string data, string machineId)
        {
            CommandLogs.Enqueue(new LogsClientStaffEntity
            {
                UserId = userId,
                DataString = data,
                MachineId = machineId,
                Timestamp = PlusEnvironment.GetUnixTimestamp()
            });
        }

        public static void LogNameChange(int userId, string newName, string oldName)
        {
            NameChangeLogs.Enqueue(new LogsClientNamechangeEntity
            {
                UserId = userId,
                NewName = newName,
                OldName = oldName,
                Timestamp = (int)PlusEnvironment.GetUnixTimestamp()
            });
        }

        public static void LogTrade(int userOneId, int userTwoId, string userOneItems, string userTwoItems)
        {
            TradeLogs.Enqueue(new LogsClientTradeEntity
            {
                _1id = userOneId,
                _2id = userTwoId,
                _1items = userOneItems,
                _2items = userTwoItems,
                Timestamp = ((long)PlusEnvironment.GetUnixTimestamp()).ToString()
            });
        }

        public static void LogConsoleInvitation(int userId, string message)
        {
            ConsoleInvitations.Enqueue(new ChatlogsConsoleInvitationEntity
            {
                UserId = userId,
                Message = message,
                Timestamp = PlusEnvironment.GetUnixTimestamp()
            });
        }

        public static void Flush()
        {
            try {
                using WavePlusContext db = PlusEnvironment.GetDbContext();
                bool dirty = false;

                dirty |= Drain(CommandLogs, rows => db.LogsClientStaffs.AddRange(rows));
                dirty |= Drain(NameChangeLogs, rows => db.LogsClientNamechanges.AddRange(rows));
                dirty |= Drain(TradeLogs, rows => db.LogsClientTrades.AddRange(rows));
                dirty |= Drain(ConsoleInvitations, rows => db.ChatlogsConsoleInvitations.AddRange(rows));

                if (dirty)
                    db.SaveChanges();
            } catch (Exception ex) {
                Log.Error($"LogBuffer flush failed: {ex.Message}");
            }
        }

        private static bool Drain<T>(ConcurrentQueue<T> queue, Action<List<T>> addRange)
        {
            if (queue.IsEmpty)
                return false;

            List<T> rows = [];
            while (queue.TryDequeue(out T row))
                rows.Add(row);

            if (rows.Count == 0)
                return false;

            addRange(rows);
            return true;
        }
    }
}