using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using log4net;
using Plus.Database.Interfaces;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
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

            using IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();

            dbClient.SetQuery("SELECT * FROM `rp_crimes`");
            DataTable crimes = dbClient.GetTable();
            if (crimes != null) {
                foreach (DataRow row in crimes.Rows)
                {
                    RpCrime crime = new(
                        Convert.ToInt32(row["id"]),
                        Convert.ToString(row["crime_tag"]),
                        Convert.ToString(row["crime_name"]),
                        Convert.ToInt32(row["severity"]),
                        Convert.ToInt32(row["time_active"]),
                        Convert.ToInt32(row["automatic_report"]) == 1,
                        Convert.ToInt32(row["police_alert"]) == 1,
                        Convert.ToString(row["note"]));

                    _byId[crime.Id] = crime;
                    _byTag[crime.Tag] = crime;
                }
            }

            dbClient.SetQuery("SELECT * FROM `rp_crime_penalties` ORDER BY `star_level` ASC");
            DataTable penalties = dbClient.GetTable();
            if (penalties != null) {
                foreach (DataRow row in penalties.Rows)
                {
                    _penalties.Add(new RpCrimePenalty(
                        Convert.ToInt32(row["id"]),
                        Convert.ToInt32(row["star_level"]),
                        Convert.ToInt32(row["fine_amount"]),
                        Convert.ToInt32(row["jail_time"])));
                }
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
        public void Commit(Habbo offender, string crimeTag, Room room)
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
                    // Every committed crime is charged/visible regardless of the crime's
                    // automatic_report flag — committing it is enough to be wanted for it.
                    AutoReported = true,
                    Revealed = false,
                    CommittedAt = now,
                    ExpiresAt = now.AddMinutes(Math.Max(1, crime.TimeActiveMinutes)),
                    Log = log
                });
            }

            if (crime.PoliceAlert)
                AlertPolice(offender, crime, room);
        }

        private static void AlertPolice(Habbo offender, RpCrime crime, Room room)
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
                    if (rec.Crimes[i].ExpiresAt <= now) {
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
                using IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
                foreach (PendingCrimeLog log in logs)
                {
                    dbClient.SetQuery("INSERT INTO `rp_crime_log` (`crime_id`, `user_id`, `timestamp`, `expired`) VALUES (@cid, @uid, @ts, @exp)");
                    dbClient.AddParameter("cid", log.CrimeId);
                    dbClient.AddParameter("uid", userId);
                    dbClient.AddParameter("ts", log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));
                    dbClient.AddParameter("exp", log.Expired ? 1 : 0);
                    dbClient.RunQuery();
                }
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
