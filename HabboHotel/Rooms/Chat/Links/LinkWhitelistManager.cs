using System.Collections.Generic;
using System.Linq;
using System.Threading;
using log4net;
using Plus.Database.EF;
using Plus.Database.EF.Entities;

namespace Plus.HabboHotel.Rooms.Chat.Links
{
    public sealed class LinkWhitelistManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LinkWhitelistManager));

        public readonly struct Entry(string pattern, string matchType)
        {
            public string Pattern { get; } = pattern;
            public string MatchType { get; } = matchType;
        }

        private readonly Lock _sync = new();
        private List<Entry> _entries = [];

        public void Init()
        {
            List<Entry> loaded = [];

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var rows = db.RpLinkWhitelist
                    .Where(r => r.Enabled == 1)
                    .Select(r => new { r.Pattern, r.MatchType })
                    .ToList();

                foreach (var row in rows) {
                    if (string.IsNullOrWhiteSpace(row.Pattern))
                        continue;

                    loaded.Add(new Entry(row.Pattern.Trim(), NormaliseType(row.MatchType)));
                }
            }

            lock (_sync) {
                _entries = loaded;
            }

            Log.Info($"Link Whitelist Manager -> LOADED ({loaded.Count} links)");
        }

        public IReadOnlyList<Entry> GetEntries()
        {
            lock (_sync) {
                return _entries.ToList();
            }
        }

        public bool Add(string pattern, string matchType)
        {
            pattern = (pattern ?? string.Empty).Trim();
            matchType = NormaliseType(matchType);
            if (string.IsNullOrWhiteSpace(pattern))
                return false;

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                RpLinkWhitelistEntity row = db.RpLinkWhitelist
                    .FirstOrDefault(r => r.Pattern == pattern && r.MatchType == matchType);

                if (row == null) {
                    db.RpLinkWhitelist.Add(new RpLinkWhitelistEntity
                    {
                        Pattern = pattern,
                        MatchType = matchType,
                        Enabled = 1
                    });
                } else if (row.Enabled == 1) {
                    return false;
                } else {
                    row.Enabled = 1;
                }

                db.SaveChanges();
            }

            Init();
            return true;
        }

        public bool Remove(string pattern)
        {
            pattern = (pattern ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(pattern))
                return false;

            int removed;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                List<RpLinkWhitelistEntity> rows = [.. db.RpLinkWhitelist.Where(r => r.Pattern == pattern)];

                if (rows.Count == 0)
                    return false;

                db.RpLinkWhitelist.RemoveRange(rows);
                removed = db.SaveChanges();
            }

            Init();
            return removed > 0;
        }

        private static string NormaliseType(string matchType)
        {
            return (matchType ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "wildcard" => "wildcard",
                "prefix" => "prefix",
                _ => "domain"
            };
        }
    }
}