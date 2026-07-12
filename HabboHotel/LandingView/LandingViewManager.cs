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

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var rows = db.server_landings.OrderByDescending(s => s.id)
                    .Select(s => new { s.id, s.title, s.text, s.button_text, s.button_type, s.button_link, s.image_link })
                    .ToList();
                foreach (var row in rows)
                {
                    _promotionItems.Add(row.id, new Promotion(row.id, row.title, row.text, row.button_text, Convert.ToInt32(row.button_type), row.button_link, row.image_link));
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
