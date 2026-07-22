using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.Database.EF.Entities;

namespace Plus.HabboHotel.Moderation
{
    public sealed class ModerationManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ModerationManager));

        private int _ticketCount = 1;
        private readonly List<string> _userPresets = new();
        private readonly List<string> _roomPresets = new();
        private readonly Dictionary<string, ModerationBan> _bans = new();
        private readonly Dictionary<int, string> _userActionPresetCategories = new();
        private readonly Dictionary<int, List<ModerationPresetActionMessages>> _userActionPresetMessages = new();
        private readonly ConcurrentDictionary<int, ModerationTicket> _modTickets = new();

        private readonly Dictionary<int, string> _moderationCfhTopics = new();
        private readonly Dictionary<int, List<ModerationPresetActions>> _moderationCfhTopicActions = new();

        public void Init()
        {
            if (_userPresets.Count > 0)
                _userPresets.Clear();
            if (_moderationCfhTopics.Count > 0)
                _moderationCfhTopics.Clear();
            if (_moderationCfhTopicActions.Count > 0)
                _moderationCfhTopicActions.Clear();
            if (_bans.Count > 0)
                _bans.Clear();

            using WavePlusContext db = PlusEnvironment.GetDbContext();

            foreach (var row in db.ModerationPresets.Select(p => new { p.Type, p.Message }).ToList()) {
                switch (row.Type?.ToLower()) {
                    case "user":
                        _userPresets.Add(row.Message);
                        break;

                    case "room":
                        _roomPresets.Add(row.Message);
                        break;
                }
            }

            foreach (var row in db.ModerationTopics.Select(t => new { t.Id, t.Caption }).ToList()) {
                if (!_moderationCfhTopics.ContainsKey((int)row.Id))
                    _moderationCfhTopics.Add((int)row.Id, row.Caption);
            }

            foreach (var row in db.ModerationTopicActions.Select(a => new { a.Id, a.ParentId, a.Type, a.Caption, a.MessageText, a.MuteTime, a.BanTime, a.IpTime, a.TradeLockTime, a.DefaultSanction }).ToList()) {
                int parentId = row.ParentId;

                if (!_moderationCfhTopicActions.ContainsKey(parentId))
                    _moderationCfhTopicActions.Add(parentId, new List<ModerationPresetActions>());

                _moderationCfhTopicActions[parentId].Add(new ModerationPresetActions((int)row.Id, row.ParentId, row.Type, row.Caption, row.MessageText,
                    row.MuteTime, row.BanTime, row.IpTime, row.TradeLockTime, row.DefaultSanction));
            }

            foreach (var row in db.ModerationPresetActionCategories.Select(c => new { c.Id, c.Caption }).ToList())
                _userActionPresetCategories.Add((int)row.Id, row.Caption);

            foreach (var row in db.ModerationPresetActionMessages.Select(m => new { m.Id, m.ParentId, m.Caption, m.MessageText, m.MuteHours, m.BanHours, m.IpBanHours, m.TradeLockDays, m.Notice }).ToList()) {
                int parentId = (int)row.ParentId;

                if (!_userActionPresetMessages.ContainsKey(parentId))
                    _userActionPresetMessages.Add(parentId, new List<ModerationPresetActionMessages>());

                _userActionPresetMessages[parentId].Add(new ModerationPresetActionMessages((int)row.Id, (int)row.ParentId, row.Caption, row.MessageText,
                    row.MuteHours, row.BanHours, row.IpBanHours, row.TradeLockDays, row.Notice));
            }

            LoadBans(db);

            Log.Info("Loaded " + (_userPresets.Count + _roomPresets.Count) + " moderation presets.");
            Log.Info("Loaded " + _userActionPresetCategories.Count + " moderation categories.");
            Log.Info("Loaded " + _userActionPresetMessages.Count + " moderation action preset messages.");
            Log.Info("Cached " + _bans.Count + " username and machine bans.");
        }

        public void ReCacheBans()
        {
            if (_bans.Count > 0)
                _bans.Clear();

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            LoadBans(db);

            Log.Info("Cached " + _bans.Count + " username and machine bans.");
        }

        /// <summary>Loads the cached machine/user bans, purging any that have already expired.</summary>
        private void LoadBans(WavePlusContext db)
        {
            var rows = db.Bans.Where(b => b.Bantype == "machine" || b.Bantype == "user")
                .Select(b => new { b.Bantype, b.Value, b.Reason, b.Expire }).ToList();

            foreach (var row in rows) {
                string value = row.Value;
                double expires = row.Expire;

                ModerationBan ban = new(BanTypeUtility.GetModerationBanType(row.Bantype), value, row.Reason, expires);
                if (expires > PlusEnvironment.GetUnixTimestamp()) {
                    if (value != null && !_bans.ContainsKey(value))
                        _bans.Add(value, ban);
                }
                else {
                    string banType = BanTypeUtility.FromModerationBanType(ban.Type);
                    db.Bans.Where(b => b.Bantype == banType && b.Value == value).ExecuteDelete();
                }
            }
        }

        public void BanUser(string mod, ModerationBanType type, string banValue, string reason, double expireTimestamp)
        {
            string banType = type == ModerationBanType.Ip ? "ip" : type == ModerationBanType.Machine ? "machine" : "user";
            double timestamp = PlusEnvironment.GetUnixTimestamp();

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                db.Bans.Upsert(new BanEntity
                {
                    Bantype = banType,
                    Value = banValue,
                    Reason = reason,
                    Expire = expireTimestamp,
                    AddedBy = mod,
                    AddedDate = timestamp.ToString(),
                    AppealState = "0"
                }).On(b => new { b.Bantype, b.Value }).Run();

            if (type == ModerationBanType.Machine || type == ModerationBanType.Username) {
                if (!_bans.ContainsKey(banValue))
                    _bans.Add(banValue, new ModerationBan(type, banValue, reason, expireTimestamp));
            }
        }

        public ICollection<string> UserMessagePresets => _userPresets;

        public ICollection<string> RoomMessagePresets => _roomPresets;

        public ICollection<ModerationTicket> GetTickets => _modTickets.Values;

        public Dictionary<string, List<ModerationPresetActions>> UserActionPresets
        {
            get
            {
                Dictionary<string, List<ModerationPresetActions>> result = new();
                foreach (KeyValuePair<int, string> category in _moderationCfhTopics.ToList()) {
                    result.Add(category.Value, new List<ModerationPresetActions>());

                    if (_moderationCfhTopicActions.ContainsKey(category.Key)) {
                        foreach (ModerationPresetActions data in _moderationCfhTopicActions[category.Key]) {
                            result[category.Value].Add(data);
                        }
                    }
                }

                return result;
            }
        }

        public bool TryAddTicket(ModerationTicket ticket)
        {
            ticket.Id = _ticketCount++;
            return _modTickets.TryAdd(ticket.Id, ticket);
        }

        public bool TryGetTicket(int ticketId, out ModerationTicket ticket)
        {
            return _modTickets.TryGetValue(ticketId, out ticket);
        }

        public bool UserHasTickets(int userId)
        {
            return _modTickets.Any(x => x.Value.Sender.Id == userId && x.Value.Answered == false);
        }

        public ModerationTicket GetTicketBySenderId(int userId)
        {
            return _modTickets.FirstOrDefault(x => x.Value.Sender.Id == userId).Value;
        }

        /// <summary>
        /// Runs a quick check to see if a ban record is cached in the server.
        /// </summary>
        public bool IsBanned(string key, out ModerationBan ban)
        {
            if (_bans.TryGetValue(key, out ban)) {
                if (!ban.Expired)
                    return true;

                //This ban has expired, let us quickly remove it here.
                string banType = BanTypeUtility.FromModerationBanType(ban.Type);
                using (WavePlusContext db = PlusEnvironment.GetDbContext())
                    db.Bans.Where(b => b.Bantype == banType && b.Value == key).ExecuteDelete();

                //And finally, let us remove the ban record from the cache.
                if (_bans.ContainsKey(key))
                    _bans.Remove(key);
                return false;
            }

            return false;
        }

        /// <summary>
        /// Run a quick database check to see if this ban exists in the database.
        /// </summary>
        public bool MachineBanCheck(string machineId)
        {
            if (PlusEnvironment.GetGame().GetModerationManager().IsBanned(machineId, out ModerationBan _)) {
                using WavePlusContext db = PlusEnvironment.GetDbContext();

                //If there is no more ban record, then we can simply remove it from our cache!
                if (!db.Bans.Any(b => b.Bantype == "machine" && b.Value == machineId)) {
                    PlusEnvironment.GetGame().GetModerationManager().RemoveBan(machineId);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Run a quick database check to see if this ban exists in the database.
        /// </summary>
        public bool UsernameBanCheck(string username)
        {
            if (PlusEnvironment.GetGame().GetModerationManager().IsBanned(username, out ModerationBan _)) {
                using WavePlusContext db = PlusEnvironment.GetDbContext();

                //If there is no more ban record, then we can simply remove it from our cache!
                if (!db.Bans.Any(b => b.Bantype == "user" && b.Value == username)) {
                    PlusEnvironment.GetGame().GetModerationManager().RemoveBan(username);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Remove a ban from the cache based on a given value.
        /// </summary>
        public void RemoveBan(string value)
        {
            _bans.Remove(value);
        }
    }
}