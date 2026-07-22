using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Plus.Database.EF;
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

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var rows = db.ServerLandings.OrderByDescending(s => s.Id)
                    .Select(s => new { s.Id, s.Title, s.Text, s.ButtonText, s.ButtonType, s.ButtonLink, s.ImageLink })
                    .ToList();
                foreach (var row in rows) {
                    _promotionItems.Add(row.Id, new Promotion(row.Id, row.Title, row.Text, row.ButtonText, Convert.ToInt32(row.ButtonType), row.ButtonLink, row.ImageLink));
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