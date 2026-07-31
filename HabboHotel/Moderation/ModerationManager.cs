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
        private readonly List<string> _userPresets = [];
        private readonly List<string> _roomPresets = [];
        private readonly Dictionary<int, ModerationBan> _accountBans = [];
        private readonly Dictionary<string, ModerationBan> _ipBans = [];
        private readonly Dictionary<int, string> _userActionPresetCategories = [];
        private readonly Dictionary<int, List<ModerationPresetActionMessages>> _userActionPresetMessages = [];
        private readonly ConcurrentDictionary<int, ModerationTicket> _modTickets = new();

        private readonly Dictionary<int, string> _moderationCfhTopics = [];
        private readonly Dictionary<int, List<ModerationPresetActions>> _moderationCfhTopicActions = [];

        public void Init()
        {
            if (_userPresets.Count > 0)
                _userPresets.Clear();
            if (_moderationCfhTopics.Count > 0)
                _moderationCfhTopics.Clear();
            if (_moderationCfhTopicActions.Count > 0)
                _moderationCfhTopicActions.Clear();
            _accountBans.Clear();
            _ipBans.Clear();

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
            Log.Info("Cached " + (_accountBans.Count + _ipBans.Count) + " bans.");
        }

        public void ReCacheBans()
        {
            _accountBans.Clear();
            _ipBans.Clear();

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            LoadBans(db);

            Log.Info("Cached " + (_accountBans.Count + _ipBans.Count) + " bans.");
        }

        private void LoadBans(WavePlusContext db)
        {
            var rows = db.Bans.Where(b => b.Bantype == "account" || b.Bantype == "ip")
                .Select(b => new { b.Bantype, b.Value, b.Reason, b.Expire, b.UserId }).ToList();

            foreach (var row in rows) {
                // Expired bans are left in the table (history for the website) — just not cached.
                if (row.Expire <= PlusEnvironment.GetUnixTimestamp())
                    continue;

                ModerationBanType type = BanTypeUtility.GetModerationBanType(row.Bantype);
                ModerationBan ban = new(type, row.Value, row.Reason, row.Expire, row.UserId);

                if (type == ModerationBanType.Ip && !string.IsNullOrEmpty(row.Value))
                    _ipBans[row.Value] = ban;
                if (row.UserId > 0)
                    _accountBans[row.UserId] = ban;
            }
        }

        public void BanUser(string mod, ModerationBanType type, string banValue, string reason, double expireTimestamp, int userId = 0)
        {
            string banType = type == ModerationBanType.Ip ? "ip" : "account";
            banValue ??= string.Empty;
            double timestamp = PlusEnvironment.GetUnixTimestamp();

            // Manual upsert on (user_id, bantype). FlexLabs' .On() couldn't match the model here and
            // aborted the insert, so we do the find-or-update ourselves — one row per user per type.
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                BanEntity existing = db.Bans.FirstOrDefault(b => b.UserId == userId && b.Bantype == banType);
                if (existing != null) {
                    existing.Value = banValue;
                    existing.Reason = reason;
                    existing.Expire = expireTimestamp;
                    existing.AddedBy = mod;
                    existing.AddedDate = timestamp.ToString();
                    existing.AppealState = "0";
                } else {
                    db.Bans.Add(new BanEntity
                    {
                        UserId = userId,
                        Bantype = banType,
                        Value = banValue,
                        Reason = reason,
                        Expire = expireTimestamp,
                        AddedBy = mod,
                        AddedDate = timestamp.ToString(),
                        AppealState = "0"
                    });
                }

                db.SaveChanges();
            }

            ModerationBan ban = new(type, banValue, reason, expireTimestamp, userId);
            if (type == ModerationBanType.Ip)
                _ipBans[banValue] = ban;
            if (userId > 0)
                _accountBans[userId] = ban;
        }

        public bool IsUserBanned(int userId, out ModerationBan ban)
        {
            if (_accountBans.TryGetValue(userId, out ban)) {
                if (!ban.Expired)
                    return true;

                // Expired: drop from cache only, leave the row in the table.
                _accountBans.Remove(userId);
            }

            ban = null;
            return false;
        }

        public bool IsIpBanned(string ip, out ModerationBan ban)
        {
            if (!string.IsNullOrEmpty(ip) && _ipBans.TryGetValue(ip, out ban)) {
                if (!ban.Expired)
                    return true;

                // Expired: drop from cache only, leave the row in the table.
                _ipBans.Remove(ip);
            }

            ban = null;
            return false;
        }

        // :unban — lift both the account row and the ip row tied to this user id by setting their
        // expiry to now, then drop them from the caches.
        public void Unban(int userId)
        {
            double now = PlusEnvironment.GetUnixTimestamp();

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                db.Bans.Where(b => b.UserId == userId)
                    .ExecuteUpdate(s => s.SetProperty(b => b.Expire, b => now));

            _accountBans.Remove(userId);

            foreach (string key in _ipBans.Where(kvp => kvp.Value.UserId == userId).Select(kvp => kvp.Key).ToList())
                _ipBans.Remove(key);
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

    }
}