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

            foreach (string str in dataActions.Split('|')) {
                if (Actions == null) {
                    Actions = new List<string>();
                }

                Actions.Add(str);
            }

            foreach (string str in dataGifts.Split('|')) {
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
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var rows = db.TalentsSubLevels.Where(t => t.TalentLevel == level)
                    .Select(t => new { t.SubLevel, t.BadgeCode, t.RequiredProgress })
                    .ToList();
                foreach (var row in rows) {
                    _subLevels.Add(row.SubLevel, new TalentTrackSubLevel(row.SubLevel, row.BadgeCode, row.RequiredProgress));
                }
            }
        }

        public ICollection<TalentTrackSubLevel> GetSubLevels()
        {
            return _subLevels.Values;
        }
    }
}