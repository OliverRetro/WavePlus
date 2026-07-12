using System.Collections.Generic;
using System.Linq;
using log4net;
using Plus.Database.EF;

namespace Plus.HabboHotel.Subscriptions
{
    public class SubscriptionManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SubscriptionManager));

        private readonly Dictionary<int, SubscriptionData> _subscriptions = new();

        public void Init()
        {
            if (_subscriptions.Count > 0)
                _subscriptions.Clear();

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var subs = db.subscriptions
                    .Select(s => new { s.id, s.name, s.badge_code, s.credits, s.duckets, s.respects, s.static_aggression, s.trash_search_cooldown })
                    .ToList();
                foreach (var row in subs)
                {
                    if (!_subscriptions.ContainsKey(row.id))
                        _subscriptions.Add(row.id, new SubscriptionData(row.id, row.name, row.badge_code, row.credits, row.duckets, row.respects, row.static_aggression, row.trash_search_cooldown));
                }
            }

            Log.Info("Loaded " + _subscriptions.Count + " subscriptions.");
        }

        public bool TryGetSubscriptionData(int id, out SubscriptionData data)
        {
            return _subscriptions.TryGetValue(id, out data);
        }
    }
}
