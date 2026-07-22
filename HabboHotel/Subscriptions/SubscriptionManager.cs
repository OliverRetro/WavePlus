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

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var subs = db.Subscriptions
                    .Select(s => new { s.Id, s.Name, s.BadgeCode, s.Credits, s.Duckets, s.Respects, s.StaticAggression, s.TrashSearchCooldown })
                    .ToList();
                foreach (var row in subs) {
                    if (!_subscriptions.ContainsKey(row.Id))
                        _subscriptions.Add(row.Id, new SubscriptionData(row.Id, row.Name, row.BadgeCode, row.Credits, row.Duckets, row.Respects, row.StaticAggression, row.TrashSearchCooldown));
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