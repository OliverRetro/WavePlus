using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.Database.EF.Entities;

namespace Plus.Core.Persistence
{
    public static class RoomVisitBuffer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RoomVisitBuffer));

        private static readonly ConcurrentDictionary<int, UserRoomvisitEntity> CurrentVisit = new();
        private static readonly ConcurrentQueue<UserRoomvisitEntity> PendingInserts = new();
        private static readonly ConcurrentQueue<(int VisitId, double ExitTimestamp)> PendingExitUpdates = new();

        public static void OnEnter(int userId, int roomId)
        {
            var visit = new UserRoomvisitEntity
            {
                UserId = (uint)userId,
                RoomId = (uint)roomId,
                EntryTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ExitTimestamp = 0,
                Hour = DateTime.Now.Hour,
                Minute = DateTime.Now.Minute
            };

            CurrentVisit[userId] = visit;
            PendingInserts.Enqueue(visit);
        }

        public static void OnExit(int userId)
        {
            if (!CurrentVisit.TryRemove(userId, out UserRoomvisitEntity visit))
                return;

            double exitTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // If the row hasn't been inserted yet the exit timestamp simply rides along with
            // the insert; once it has a database id we queue an explicit update.
            visit.ExitTimestamp = exitTimestamp;
            if (visit.Id != 0)
                PendingExitUpdates.Enqueue((visit.Id, exitTimestamp));
        }

        public static void Flush()
        {
            try {
                List<UserRoomvisitEntity> inserts = null;
                while (PendingInserts.TryDequeue(out UserRoomvisitEntity visit))
                    (inserts ??= []).Add(visit);

                if (inserts != null) {
                    using WavePlusContext db = PlusEnvironment.GetDbContext();
                    db.UserRoomvisits.AddRange(inserts);
                    db.SaveChanges();
                }

                if (!PendingExitUpdates.IsEmpty) {
                    using WavePlusContext db = PlusEnvironment.GetDbContext();
                    while (PendingExitUpdates.TryDequeue(out (int VisitId, double ExitTimestamp) update)) {
                        int visitId = update.VisitId;
                        double exitTimestamp = update.ExitTimestamp;
                        db.UserRoomvisits.Where(v => v.Id == visitId)
                            .ExecuteUpdate(s => s.SetProperty(v => v.ExitTimestamp, exitTimestamp));
                    }
                }
            } catch (Exception ex) {
                Log.Error($"RoomVisitBuffer flush failed: {ex.Message}");
            }
        }
    }
}