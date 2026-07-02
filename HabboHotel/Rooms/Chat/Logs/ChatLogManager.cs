using System.Collections.Generic;
using System.Threading;
using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Rooms.Chat.Logs
{
    public sealed class ChatLogManager
    {
        private const int FlushOnCount = 10;

        private readonly List<ChatLogEntry> _chatLogs;
        private readonly ReaderWriterLockSlim _lock;

        public ChatLogManager()
        {
            _chatLogs = new List<ChatLogEntry>();
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        }

        public void StoreChatLog(ChatLogEntry entry)
        {
            _lock.EnterUpgradeableReadLock();

            _chatLogs.Add(entry);

            OnChatLogStore();

            _lock.ExitUpgradeableReadLock();
        }

        private void OnChatLogStore()
        {
            if (_chatLogs.Count >= FlushOnCount)
                FlushAndSave();
        }

        public void FlushAndSave()
        {
            _lock.EnterWriteLock();

            if (_chatLogs.Count > 0) {
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    foreach (ChatLogEntry entry in _chatLogs)
                    {
                        dbClient.SetQuery("INSERT INTO chatlogs (`user_id`, `room_id`, `timestamp`, `message`) VALUES (@uid, @rid, @time, @msg)");
                        dbClient.AddParameter("uid", entry.PlayerId);
                        dbClient.AddParameter("rid", entry.RoomId);
                        dbClient.AddParameter("time", entry.Timestamp);
                        dbClient.AddParameter("msg", entry.Message);
                        dbClient.RunQuery();
                    }
                }
            }

            _chatLogs.Clear();
            _lock.ExitWriteLock();
        }
    }
}