using System;
using System.Collections.Generic;
using System.Data;
using log4net;
using Plus.Database.Interfaces;
using Plus.HabboHotel.LandingView.Promotions;

namespace Plus.HabboHotel.LandingView
{
    public class LandingViewManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LandingViewManager));

        private readonly Dictionary<int, Promotion> _promotionItems;

        public LandingViewManager()
        {
            _promotionItems = new Dictionary<int, Promotion>();
        }

        public void Init()
        {
            if (_promotionItems.Count > 0)
                _promotionItems.Clear();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `server_landing` ORDER BY `id` DESC");
                DataTable getData = dbClient.GetTable();

                if (getData != null) {
                    foreach (DataRow row in getData.Rows)
                    {
                        _promotionItems.Add(Convert.ToInt32(row[0]), new Promotion((int) row[0], row[1].ToString(), row[2].ToString(), row[3].ToString(), Convert.ToInt32(row[4]), row[5].ToString(), row[6].ToString()));
                    }
                }
            }


            Log.Info("Landing View Manager -> LOADED");
        }

        public ICollection<Promotion> GetPromotionItems()
        {
            return _promotionItems.Values;
        }
    }
}