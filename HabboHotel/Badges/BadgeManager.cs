using System.Collections.Generic;
using System.Linq;
using log4net;
using Plus.Database.EF;

namespace Plus.HabboHotel.Badges
{
    public class BadgeManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BadgeManager));

        private readonly Dictionary<string, BadgeDefinition> _badges;

        public BadgeManager()
        {
            _badges = new Dictionary<string, BadgeDefinition>();
        }

        public void Init()
        {
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var rows = db.BadgeDefinitions.Select(b => new { b.Code, b.RequiredRight }).ToList();
                foreach (var row in rows) {
                    string code = row.Code.ToUpper();

                    if (!_badges.ContainsKey(code))
                        _badges.Add(code, new BadgeDefinition(code, row.RequiredRight));
                }
            }

            Log.Info("Loaded " + _badges.Count + " badge definitions.");
        }

        public bool TryGetBadge(string code, out BadgeDefinition badge)
        {
            return _badges.TryGetValue(code.ToUpper(), out badge);
        }
    }
}