using System;
using System.Collections.Generic;
using System.Data;
using log4net;
using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Rooms.Chat.Styles
{
    public sealed class ChatStyleManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ChatStyleManager));

        private readonly Dictionary<int, ChatStyle> _styles;

        public ChatStyleManager()
        {
            _styles = new Dictionary<int, ChatStyle>();
        }

        public void Init()
        {
            if (_styles.Count > 0)
                _styles.Clear();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `room_chat_styles`;");
                DataTable table = dbClient.GetTable();

                if (table != null) {
                    foreach (DataRow row in table.Rows)
                    {
                        try
                        {
                            if (!_styles.ContainsKey(Convert.ToInt32(row["id"])))
                                _styles.Add(Convert.ToInt32(row["id"]), new ChatStyle(Convert.ToInt32(row["id"]), Convert.ToString(row["name"]), Convert.ToString(row["required_right"])));
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Unable to load ChatBubble for ID [" + Convert.ToInt32(row["id"]) + "]", ex);
                        }
                    }
                }
            }

            Log.Info("Loaded " + _styles.Count + " chat styles.");
        }

        public bool TryGetStyle(int id, out ChatStyle style)
        {
            return _styles.TryGetValue(id, out style);
        }
    }
}