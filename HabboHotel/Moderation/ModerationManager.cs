using System;
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

            foreach (var row in db.moderation_presets.Select(p => new { p.type, p.message }).ToList())
            {
                switch (row.type?.ToLower())
                {
                    case "user":
                        _userPresets.Add(row.message);
                        break;

                    case "room":
                        _roomPresets.Add(row.message);
                        break;
                }
            }

            foreach (var row in db.moderation_topics.Select(t => new { t.id, t.caption }).ToList())
            {
                if (!_moderationCfhTopics.ContainsKey((int)row.id))
                    _moderationCfhTopics.Add((int)row.id, row.caption);
            }

            foreach (var row in db.moderation_topic_actions.Select(a => new { a.id, a.parent_id, a.type, a.caption, a.message_text, a.mute_time, a.ban_time, a.ip_time, a.trade_lock_time, a.default_sanction }).ToList())
            {
                int parentId = row.parent_id;

                if (!_moderationCfhTopicActions.ContainsKey(parentId))
                    _moderationCfhTopicActions.Add(parentId, new List<ModerationPresetActions>());

                _moderationCfhTopicActions[parentId].Add(new ModerationPresetActions((int)row.id, row.parent_id, row.type, row.caption, row.message_text,
                    row.mute_time, row.ban_time, row.ip_time, row.trade_lock_time, row.default_sanction));
            }

            foreach (var row in db.moderation_preset_action_categories.Select(c => new { c.id, c.caption }).ToList())
                _userActionPresetCategories.Add((int)row.id, row.caption);

            foreach (var row in db.moderation_preset_action_messages.Select(m => new { m.id, m.parent_id, m.caption, m.message_text, m.mute_hours, m.ban_hours, m.ip_ban_hours, m.trade_lock_days, m.notice }).ToList())
            {
                int parentId = (int)row.parent_id;

                if (!_userActionPresetMessages.ContainsKey(parentId))
                    _userActionPresetMessages.Add(parentId, new List<ModerationPresetActionMessages>());

                _userActionPresetMessages[parentId].Add(new ModerationPresetActionMessages((int)row.id, (int)row.parent_id, row.caption, row.message_text,
                    row.mute_hours, row.ban_hours, row.ip_ban_hours, row.trade_lock_days, row.notice));
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
            var rows = db.bans.Where(b => b.bantype == "machine" || b.bantype == "user")
                .Select(b => new { b.bantype, b.value, b.reason, b.expire }).ToList();

            foreach (var row in rows)
            {
                string value = row.value;
                double expires = row.expire;

                ModerationBan ban = new(BanTypeUtility.GetModerationBanType(row.bantype), value, row.reason, expires);
                if (expires > PlusEnvironment.GetUnixTimestamp()) {
                    if (value != null && !_bans.ContainsKey(value))
                        _bans.Add(value, ban);
                } else {
                    string banType = BanTypeUtility.FromModerationBanType(ban.Type);
                    db.bans.Where(b => b.bantype == banType && b.value == value).ExecuteDelete();
                }
            }
        }

        public void BanUser(string mod, ModerationBanType type, string banValue, string reason, double expireTimestamp)
        {
            string banType = type == ModerationBanType.Ip ? "ip" : type == ModerationBanType.Machine ? "machine" : "user";
            double timestamp = PlusEnvironment.GetUnixTimestamp();

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                db.bans.Upsert(new ban
                {
                    bantype = banType,
                    value = banValue,
                    reason = reason,
                    expire = expireTimestamp,
                    added_by = mod,
                    added_date = timestamp.ToString(),
                    appeal_state = "0"
                }).On(b => new { b.bantype, b.value }).Run();

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
                foreach (KeyValuePair<int, string> category in _moderationCfhTopics.ToList())
                {
                    result.Add(category.Value, new List<ModerationPresetActions>());

                    if (_moderationCfhTopicActions.ContainsKey(category.Key)) {
                        foreach (ModerationPresetActions data in _moderationCfhTopicActions[category.Key])
                        {
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
                    db.bans.Where(b => b.bantype == banType && b.value == key).ExecuteDelete();

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
                if (!db.bans.Any(b => b.bantype == "machine" && b.value == machineId)) {
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
                if (!db.bans.Any(b => b.bantype == "user" && b.value == username)) {
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
