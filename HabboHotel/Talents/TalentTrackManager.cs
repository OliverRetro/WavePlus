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
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var rows = db.talents.Select(t => new { t.type, t.level, t.data_actions, t.data_gifts }).ToList();
                foreach (var row in rows)
                {
                    int level = row.level ?? 0;
                    _citizenshipLevels.Add(level, new TalentTrackLevel(row.type, level, row.data_actions, row.data_gifts));
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
