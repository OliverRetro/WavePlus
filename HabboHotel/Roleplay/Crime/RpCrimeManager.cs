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
    /// <summary>
    /// Loads crime/penalty definitions and tracks every player's live wanted state.
    /// The wanted list is cached in memory only; crime logs are buffered and flushed to
    /// `rp_crime_log` on disconnect / reload.
    /// </summary>
    public sealed class RpCrimeManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RpCrimeManager));

        private readonly Dictionary<int, RpCrime> _byId = new();
        private readonly Dictionary<string, RpCrime> _byTag = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<RpCrimePenalty> _penalties = new();

        private readonly ConcurrentDictionary<int, UserWantedRecord> _wanted = new();
        private readonly ConcurrentDictionary<int, List<PendingCrimeLog>> _logBuffer = new();

        /// <summary>Maximum reachable star level (= number of penalty rows).</summary>
        public int MaxStars => _penalties.Count;

        public void Init()
        {
            _byId.Clear();
            _byTag.Clear();
            _penalties.Clear();

            using WavePlusContext db = PlusEnvironment.GetDbContext();

            foreach (var row in db.RpCrimes.AsNoTracking().ToList())
            {
                RpCrime crime = new(
                    row.Id,
                    row.CrimeTag,
                    row.CrimeName,
                    row.Severity,
                    row.TimeActive,
                    row.AutomaticReport == 1,
                    row.PoliceAlert == 1,
                    row.Note);

                _byId[crime.Id] = crime;
                _byTag[crime.Tag] = crime;
            }

            foreach (var row in db.RpCrimePenalties.AsNoTracking().OrderBy(x => x.StarLevel).ToList())
            {
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

        /// <summary>Highest penalty whose star level is at or below the given stars.</summary>
        public RpCrimePenalty GetPenalty(int starLevel)
        {
            RpCrimePenalty best = null;
            foreach (RpCrimePenalty p in _penalties)
                if (p.StarLevel <= starLevel && (best == null || p.StarLevel > best.StarLevel))
                    best = p;
            return best;
        }

        /// <summary>Records a crime committed by the offender, alerting police if configured.</summary>
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

            PendingCrimeLog log = new() { CrimeId = crime.Id, Timestamp = now, Expired = false };
            _logBuffer.GetOrAdd(offender.Id, _ => new List<PendingCrimeLog>()).Add(log);

            lock (rec.Crimes)
            {
                rec.Crimes.Add(new ActiveCrime
                {
                    CrimeId = crime.Id,
                    Severity = crime.Severity,
                    // Auto-reported crimes are instantly visible (and livefed) — hidden crimes
                    // stay invisible until a police officer runs `:charge <user> <crime>`.
                    AutoReported = crime.AutomaticReport,
                    Revealed = false,
                    CommittedAt = now,
                    ExpiresAt = now.AddMinutes(Math.Max(1, crime.TimeActiveMinutes)),
                    Log = log
                });
            }

            if (crime.PoliceAlert)
                AlertPolice(offender, crime, room);

            // Only auto-reported crimes are announced publicly on the livefeed.
            if (crime.AutomaticReport)
            {
                string stars = crime.Severity > 0 ? " " + new string('⭐', Math.Min(5, crime.Severity)) : "";
                LiveFeedService.LiveFeed("[AUTO CHARGE] 🚨 " + LiveFeedService.Name(offender.Username, "green") + " committed " + crime.Name + stars);
            }
        }

        /// <summary>
        /// Resolves a crime from a free-text query (its name, tag, or tag with spaces).
        /// e.g. "Cop Assault", "cop_assault" and "cop assault" all resolve the same crime.
        /// </summary>
        public RpCrime ResolveCrime(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return null;

            string q = query.Trim();
            foreach (RpCrime crime in _byId.Values)
            {
                if (string.Equals(crime.Name, q, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(crime.Tag, q, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(crime.Tag.Replace('_', ' '), q, StringComparison.OrdinalIgnoreCase))
                    return crime;
            }
            return null;
        }

        /// <summary>
        /// Charges a suspect with a specific crime: reveals every still-active instance of that
        /// crime (adjusting their visible stars), and flags the record as charged so they can be
        /// cuffed. Returns how many instances were newly revealed, and the resulting visible stars.
        /// </summary>
        public (int applied, int stars) ChargeCrime(int userId, RpCrime crime)
        {
            if (crime == null || !_wanted.TryGetValue(userId, out UserWantedRecord rec))
                return (0, 0);

            Prune(rec);
            int applied = 0;
            lock (rec.Crimes)
            {
                foreach (ActiveCrime c in rec.Crimes)
                {
                    if (c.CrimeId != crime.Id)
                        continue;

                    // Count every active instance of this crime; reveal any still hidden.
                    c.Revealed = true;
                    applied++;
                }

                if (applied > 0)
                    rec.Charged = true;
            }

            return (applied, GetVisibleStars(userId));
        }

        private static void AlertPolice(Habbo offender, RpCrime crime, Rooms.Room room)
        {
            if (!int.TryParse(PlusEnvironment.GetSettingsManager().TryGetValue("rp.police.corporation.id"), out int policeCorp) || policeCorp <= 0)
                return;

            string where = room != null ? $"{room.Name} (#{room.Id})" : "an unknown location";
            string message = $"[DISPATCH] {offender.Username} committed {crime.Name} in {where}.";

            foreach (int officerId in PlusEnvironment.GetGame().GetShiftManager().GetWorkersForGroup(policeCorp))
            {
                GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(officerId);
                client?.SendWhisper(message, 1);
            }
        }

        /// <summary>Stars publicly visible on the wanted list (auto-reported or revealed).</summary>
        public int GetVisibleStars(int userId)
        {
            if (!_wanted.TryGetValue(userId, out UserWantedRecord rec))
                return 0;

            Prune(rec);
            int stars;
            lock (rec.Crimes)
                stars = rec.Crimes.Where(c => c.AutoReported || c.Revealed).Sum(c => c.Severity);
            return Math.Min(MaxStars, stars);
        }

        /// <summary>All stars including hidden (non-reported) crimes — used when charging.</summary>
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

        /// <summary>Reveals every active crime (a charge exposes hidden ones too).</summary>
        public void RevealAll(int userId)
        {
            if (!_wanted.TryGetValue(userId, out UserWantedRecord rec))
                return;

            Prune(rec);
            lock (rec.Crimes)
                foreach (ActiveCrime c in rec.Crimes)
                    c.Revealed = true;
        }

        /// <summary>
        /// Charges the suspect: reveals/activates every crime and flags the record as charged.
        /// A suspect must be charged before police can cuff them. Returns the total stars charged.
        /// </summary>
        public int Charge(int userId)
        {
            if (!_wanted.TryGetValue(userId, out UserWantedRecord rec))
                return 0;

            Prune(rec);
            lock (rec.Crimes)
            {
                foreach (ActiveCrime c in rec.Crimes)
                    c.Revealed = true;
                rec.Charged = rec.Crimes.Count > 0;
            }

            return GetTotalStars(userId);
        }

        /// <summary>True once a cop has run <c>:charge</c> on the suspect and they still have active crimes.</summary>
        public bool IsCharged(int userId)
        {
            if (!_wanted.TryGetValue(userId, out UserWantedRecord rec))
                return false;

            Prune(rec);
            lock (rec.Crimes)
                return rec.Charged && rec.Crimes.Count > 0;
        }

        /// <summary>Clears active crimes once the offender has been jailed (they're resolved).</summary>
        public void ResolveAll(int userId)
        {
            if (_wanted.TryGetValue(userId, out UserWantedRecord rec))
                lock (rec.Crimes)
                {
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

        /// <summary>The wanted list ordered by visible stars (desc), highest first.</summary>
        public List<WantedRow> GetWantedList(int limit = 50)
        {
            List<WantedRow> rows = new();
            foreach (UserWantedRecord rec in _wanted.Values)
            {
                int stars = GetVisibleStars(rec.UserId);
                if (stars <= 0)
                    continue;
                rows.Add(new WantedRow(rec.Username ?? "", rec.Look ?? "", stars, rec.LastCrime));
            }

            return rows.OrderByDescending(r => r.Stars).ThenByDescending(r => r.Time).Take(limit).ToList();
        }

        /// <summary>Drops expired crimes, flagging their log entries as expired (not jailed).</summary>
        private static void Prune(UserWantedRecord rec)
        {
            DateTime now = DateTime.Now;
            lock (rec.Crimes)
            {
                for (int i = rec.Crimes.Count - 1; i >= 0; i--)
                {
                    if (rec.Crimes[i].ExpiresAt <= now)
                    {
                        if (rec.Crimes[i].Log != null)
                            rec.Crimes[i].Log.Expired = true;
                        rec.Crimes.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>Writes the user's buffered crime logs to the database (on disconnect/reload).</summary>
        public void FlushLogs(int userId)
        {
            if (!_logBuffer.TryRemove(userId, out List<PendingCrimeLog> logs) || logs.Count == 0)
                return;

            try
            {
                using WavePlusContext db = PlusEnvironment.GetDbContext();
                foreach (PendingCrimeLog log in logs)
                {
                    db.RpCrimeLogs.Add(new RpCrimeLogEntity
                    {
                        CrimeId = log.CrimeId,
                        UserId = userId,
                        Timestamp = log.Timestamp,
                        Expired = (sbyte)(log.Expired ? 1 : 0)
                    });
                }
                db.SaveChanges();
            }
            catch (Exception e)
            {
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
            public bool AutoReported;
            public bool Revealed;
            public DateTime CommittedAt;
            public DateTime ExpiresAt;
            public PendingCrimeLog Log;
        }

        private sealed class PendingCrimeLog
        {
            public int CrimeId;
            public DateTime Timestamp;
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
