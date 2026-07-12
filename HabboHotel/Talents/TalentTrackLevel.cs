using System.Collections.Generic;
using System.Linq;
using Plus.Database.EF;

namespace Plus.HabboHotel.Talents
{
    public class TalentTrackLevel
    {
        public string Type { get; set; }
        public int Level { get; set; }

        private readonly Dictionary<int, TalentTrackSubLevel> _subLevels;

        public TalentTrackLevel(string type, int level, string dataActions, string dataGifts)
        {
            Type = type;
            Level = level;

            foreach (string str in dataActions.Split('|'))
            {
                if (Actions == null) {
                    Actions = new List<string>();
                }

                Actions.Add(str);
            }

            foreach (string str in dataGifts.Split('|'))
            {
                if (Gifts == null) {
                    Gifts = new List<string>();
                }

                Gifts.Add(str);
            }

            _subLevels = new Dictionary<int, TalentTrackSubLevel>();

            Init();
        }

        public List<string> Actions { get; }

        public List<string> Gifts { get; }

        public void Init()
        {
            int level = Level;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var rows = db.talents_sub_levels.Where(t => t.talent_level == level)
                    .Select(t => new { t.sub_level, t.badge_code, t.required_progress })
                    .ToList();
                foreach (var row in rows)
                {
                    _subLevels.Add(row.sub_level, new TalentTrackSubLevel(row.sub_level, row.badge_code, row.required_progress));
                }
            }
        }

        public ICollection<TalentTrackSubLevel> GetSubLevels()
        {
            return _subLevels.Values;
        }
    }
}
