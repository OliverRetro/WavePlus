using System;
using System.Collections.Generic;
using System.Data;
using log4net;
using Plus.Database.Interfaces;

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

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `subscriptions`;");
                DataTable getSubscriptions = dbClient.GetTable();

                if (getSubscriptions != null) {
                    foreach (DataRow row in getSubscriptions.Rows)
                    {
                        if (!_subscriptions.ContainsKey(Convert.ToInt32(row["id"])))
                            _subscriptions.Add(Convert.ToInt32(row["id"]), new SubscriptionData(Convert.ToInt32(row["id"]), Convert.ToString(row["name"]), Convert.ToString(row["badge_code"]), Convert.ToInt32(row["credits"]), Convert.ToInt32(row["duckets"]), Convert.ToInt32(row["respects"]), Convert.ToInt32(row["static_aggression"])));
                    }
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