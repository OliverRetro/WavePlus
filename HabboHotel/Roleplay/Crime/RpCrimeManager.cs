using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Roleplay.Crime
{
    public sealed class RpCrimeManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RpCrimeManager));

        private readonly Dictionary<int, RpCrime> _byId = new();
        private readonly Dictionary<string, RpCrime> _byTag = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<RpCrimePenalty> _penalties = new();

        private readonly ConcurrentDictionary<int, UserWantedRecord> _wanted = new();
        private readonly ConcurrentDictionary<int, List<PendingCrimeLog>> _logBuffer = new();

        public int MaxStars => _penalties.Count;

        public void Init()
        {
            _byId.Clear();
            _byTag.Clear();
            _penalties.Clear();

            using WavePlusContext db = PlusEnvironment.GetDbContext();

            foreach (var row in db.RpCrimes.AsNoTracking().ToList()) {
                RpCrime crime = new(
                    row.Id,
                    row.CrimeTag,
                    row.CrimeName,
                    row.Severity,
                    row.TimeActive,
                    row.PoliceAlert == 1,
                    row.Note);

                _byId[crime.Id] = crime;
                _byTag[crime.Tag] = crime;
            }

            foreach (var row in db.RpCrimePenalties.AsNoTracking().OrderBy(x => x.StarLevel).ToList()) {
                _penalties.Add(new RpCrimePenalty(
                    row.Id,
                    row.StarLevel,
                    row.FineAmount,
                    row.JailTime));
            }

            Log.Info($"[RP] Loaded {_byId.Count} crimes and {_penalties.Count} crime penalties.");
        }

        public RpCrime GetByTag(string tag) => tag != null && _byTag.TryGetValue(tag, out RpCrime c) ? c : null;

        public RpCrime GetById(int id) => _byId.TryGetValue(id, out RpCrime c) ? c : null;

        public RpCrimePenalty GetPenalty(int starLevel)
        {
            RpCrimePenalty best = null;
            foreach (RpCrimePenalty p in _penalties)
                if (p.StarLevel <= starLevel && (best == null || p.StarLevel > best.StarLevel))
                    best = p;
            return best;
        }

        public void Commit(Habbo offender, string crimeTag, Rooms.Room room)
        {
            if (offender == null)
                return;

            RpCrime crime = GetByTag(crimeTag);
            if (crime == null)
                return;

            DateTime now = DateTime.Now;
            UserWantedRecord rec = _wanted.GetOrAdd(offender.Id, _ => new UserWantedRecord { UserId = offender.Id });
            rec.Username = offender.Username;
            rec.Look = offender.Look;
            rec.LastCrime = now;

            PendingCrimeLog log = new() { CrimeId = crime.Id, Timestamp = (int)PlusEnvironment.GetUnixTimestamp(), Expired = false };
            _logBuffer.GetOrAdd(offender.Id, _ => []).Add(log);

            DateTime expiresAt = now.AddMinutes(Math.Max(1, crime.TimeActiveMinutes));

            Prune(rec);
            lock (rec.Crimes) {
                ActiveCrime existing = rec.Crimes.FirstOrDefault(c => c.CrimeId == crime.Id);
                if (existing != null) {
                    // Already wanted for this crime — reset its clock and let the newest log entry
                    // own the expiry flag, but don't award another set of stars.
                    existing.CommittedAt = now;
                    existing.ExpiresAt = expiresAt;
                    existing.Log = log;
                } else if (crime.AutoCharge || !rec.Charged) {
                    rec.Crimes.Add(new ActiveCrime
                    {
                        CrimeId = crime.Id,
                        Severity = crime.Severity,
                        AutoCharged = crime.AutoCharge,
                        Revealed = false,
                        CommittedAt = now,
                        ExpiresAt = expiresAt,
                        Log = log
                    });
                }
                // else: a new manual crime piled on after they were charged — logged above, but it
                // never joins the record, so no officer can charge it and it earns no stars.

                if (crime.AutoCharge)
                    rec.Charged = true;
            }

            if (!crime.AutoCharge)
                return;

            AlertPolice(offender, crime, room);

            string stars = crime.Severity > 0 ? " " + new string('⭐', Math.Min(5, crime.Severity)) : "";
            LiveFeedService.LiveFeed("[AUTO CHARGE] 🚨 " + LiveFeedService.Name(offender.Username, "green") + " committed " + crime.Name + stars);
        }

        public bool HasActiveCrime(int userId, string crimeTag)
        {
            RpCrime crime = GetByTag(crimeTag);
            if (crime == null || !_wanted.TryGetValue(userId, out UserWantedRecord rec))
                return false;

            Prune(rec);
            lock (rec.Crimes)
                return rec.Crimes.Any(c => c.CrimeId == crime.Id);
        }

        public RpCrime ResolveCrime(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return null;

            string q = query.Trim();
            foreach (RpCrime crime in _byId.Values) {
                if (string.Equals(crime.Name, q, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(crime.Tag, q, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(crime.Tag.Replace('_', ' '), q, StringComparison.OrdinalIgnoreCase))
                    return crime;
            }
            return null;
        }

        public (bool applied, int stars) ChargeCrime(int userId, RpCrime crime)
        {
            // Auto-charge crimes charge themselves the moment they're committed; there is nothing
            // left for an officer to apply by hand.
            if (crime == null || crime.AutoCharge || !_wanted.TryGetValue(userId, out UserWantedRecord rec))
                return (false, 0);

            Prune(rec);
            bool applied = false;
            lock (rec.Crimes) {
                ActiveCrime active = rec.Crimes.FirstOrDefault(c => c.CrimeId == crime.Id);
                if (active != null) {
                    active.Revealed = true;
                    rec.Charged = true;
                    applied = true;
                }
            }

            return (applied, GetVisibleStars(userId));
        }

        private static void AlertPolice(Habbo offender, RpCrime crime, Rooms.Room room)
        {
            if (!int.TryParse(PlusEnvironment.GetSettingsManager().TryGetValue("rp.police.corporation.id"), out int policeCorp) || policeCorp <= 0)
                return;

            string where = room != null ? $"{room.Name} (#{room.Id})" : "an unknown location";
            string message = $"[DISPATCH] {offender.Username} committed {crime.Name} in {where}.";

            foreach (int officerId in PlusEnvironment.GetGame().GetShiftManager().GetWorkersForGroup(policeCorp)) {
                GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(officerId);
                client?.SendWhisper(message, 1);
            }
        }

        public int GetVisibleStars(int userId)
        {
            if (!_wanted.TryGetValue(userId, out UserWantedRecord rec))
                return 0;

            Prune(rec);
            int stars;
            lock (rec.Crimes)
                stars = rec.Crimes.Where(c => c.AutoCharged || c.Revealed).Sum(c => c.Severity);
            return Math.Min(MaxStars, stars);
        }

        public int GetTotalStars(int userId)
        {
            if (!_wanted.TryGetValue(userId, out UserWantedRecord rec))
                return 0;

            Prune(rec);
            int stars;
            lock (rec.Crimes)
                stars = rec.Crimes.Sum(c => c.Severity);
            return Math.Min(MaxStars, stars);
        }

        public void RevealAll(int userId)
        {
            if (!_wanted.TryGetValue(userId, out UserWantedRecord rec))
                return;

            Prune(rec);
            lock (rec.Crimes)
                foreach (ActiveCrime c in rec.Crimes)
                    c.Revealed = true;
        }

        public int Charge(int userId)
        {
            if (!_wanted.TryGetValue(userId, out UserWantedRecord rec))
                return 0;

            Prune(rec);
            lock (rec.Crimes) {
                foreach (ActiveCrime c in rec.Crimes)
                    c.Revealed = true;
                rec.Charged = rec.Crimes.Count > 0;
            }

            return GetTotalStars(userId);
        }

        public bool IsCharged(int userId)
        {
            if (!_wanted.TryGetValue(userId, out UserWantedRecord rec))
                return false;

            Prune(rec);
            lock (rec.Crimes)
                return rec.Charged && rec.Crimes.Count > 0;
        }

        public void ResolveAll(int userId)
        {
            if (_wanted.TryGetValue(userId, out UserWantedRecord rec))
                lock (rec.Crimes) {
                    rec.Crimes.Clear();
                    rec.Charged = false;
                }
        }

        public bool HasActiveCrimes(int userId)
        {
            if (!_wanted.TryGetValue(userId, out UserWantedRecord rec))
                return false;
            Prune(rec);
            lock (rec.Crimes)
                return rec.Crimes.Count > 0;
        }

        public List<WantedRow> GetWantedList(int limit = 50)
        {
            List<WantedRow> rows = new();
            foreach (UserWantedRecord rec in _wanted.Values) {
                int stars = GetVisibleStars(rec.UserId);
                if (stars <= 0)
                    continue;
                rows.Add(new WantedRow(rec.Username ?? "", rec.Look ?? "", stars, rec.LastCrime));
            }

            return rows.OrderByDescending(r => r.Stars).ThenByDescending(r => r.Time).Take(limit).ToList();
        }

        private static void Prune(UserWantedRecord rec)
        {
            DateTime now = DateTime.Now;
            lock (rec.Crimes) {
                for (int i = rec.Crimes.Count - 1; i >= 0; i--) {
                    if (rec.Crimes[i].ExpiresAt <= now) {
                        if (rec.Crimes[i].Log != null)
                            rec.Crimes[i].Log.Expired = true;
                        rec.Crimes.RemoveAt(i);
                    }
                }

                // Everything they were charged for has expired — they're out of the system, so a
                // fresh crime can be charged again. Without this the flag would stick forever.
                if (rec.Crimes.Count == 0)
                    rec.Charged = false;
            }
        }

        public void FlushLogs(int userId)
        {
            if (!_logBuffer.TryRemove(userId, out List<PendingCrimeLog> logs) || logs.Count == 0)
                return;

            try {
                using WavePlusContext db = PlusEnvironment.GetDbContext();
                foreach (PendingCrimeLog log in logs) {
                    db.RpCrimeLogs.Add(new RpCrimeLogEntity
                    {
                        CrimeId = log.CrimeId,
                        UserId = userId,
                        Timestamp = log.Timestamp,
                        Expired = (sbyte)(log.Expired ? 1 : 0)
                    });
                }
                db.SaveChanges();
            } catch (Exception e) {
                Log.Error("Failed flushing crime logs for user " + userId, e);
            }
        }

        private sealed class UserWantedRecord
        {
            public int UserId;
            public string Username;
            public string Look;
            public DateTime LastCrime;
            public bool Charged;
            public readonly List<ActiveCrime> Crimes = new();
        }

        private sealed class ActiveCrime
        {
            public int CrimeId;
            public int Severity;
            public bool AutoCharged;
            public bool Revealed;
            public DateTime CommittedAt;
            public DateTime ExpiresAt;
            public PendingCrimeLog Log;
        }

        private sealed class PendingCrimeLog
        {
            public int CrimeId;

            public int Timestamp;

            public bool Expired;
        }

        public readonly struct WantedRow
        {
            public WantedRow(string username, string look, int stars, DateTime time)
            {
                Username = username;
                Look = look;
                Stars = stars;
                Time = time;
            }

            public string Username { get; }
            public string Look { get; }
            public int Stars { get; }
            public DateTime Time { get; }
        }
    }
}