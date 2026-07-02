using System;
using System.Collections.Generic;
using System.Data;
using Plus.Database.Interfaces;

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
            DataTable getTable;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `sub_level`,`badge_code`,`required_progress` FROM `talents_sub_levels` WHERE `talent_level` = @TalentLevel");
                dbClient.AddParameter("TalentLevel", Level);
                getTable = dbClient.GetTable();
            }

            if (getTable != null) {
                foreach (DataRow row in getTable.Rows)
                {
                    _subLevels.Add(Convert.ToInt32(row["sub_level"]), new TalentTrackSubLevel(Convert.ToInt32(row["sub_level"]), Convert.ToString(row["badge_code"]), Convert.ToInt32(row["required_progress"])));
                }
            }
        }

        public ICollection<TalentTrackSubLevel> GetSubLevels()
        {
            return _subLevels.Values;
        }
    }
}