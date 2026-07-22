using System.Collections.Generic;
using System.Linq;
using log4net;
using Plus.Database.EF;

namespace Plus.HabboHotel.Talents
{
    public class TalentTrackManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TalentTrackManager));

        private readonly Dictionary<int, TalentTrackLevel> _citizenshipLevels;

        public TalentTrackManager()
        {
            _citizenshipLevels = new Dictionary<int, TalentTrackLevel>();
        }

        public void Init()
        {
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var rows = db.Talents.Select(t => new { t.Type, t.Level, t.DataActions, t.DataGifts }).ToList();
                foreach (var row in rows) {
                    int level = row.Level ?? 0;
                    _citizenshipLevels.Add(level, new TalentTrackLevel(row.Type, level, row.DataActions, row.DataGifts));
                }
            }

            Log.Info("Loaded " + _citizenshipLevels.Count + " talent track levels");
        }

        public ICollection<TalentTrackLevel> GetLevels()
        {
            return _citizenshipLevels.Values;
        }
    }
}