using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Plus.Database.EF;

namespace Plus.HabboHotel.Rooms.Chat.Logs
{
    public sealed class ChatLogManager
    {
        // Memory guard only — the regular flush is the 15-minute world saver (and shutdown).
        private const int FlushOnCount = 1000;

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
                using WavePlusContext db = PlusEnvironment.GetDbContext();
                db.Chatlogs.AddRange(_chatLogs.Select(entry => new Database.EF.Entities.ChatlogEntity
                {
                    UserId = (uint)entry.PlayerId,
                    RoomId = (uint)entry.RoomId,
                    Timestamp = entry.Timestamp,
                    Message = entry.Message
                }));
                db.SaveChanges();
            }

            _chatLogs.Clear();
            _lock.ExitWriteLock();
        }
    }
}