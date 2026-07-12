using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Plus.Database.EF;

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

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var rows = db.room_chat_styles.Select(s => new { s.id, s.name, s.required_right }).ToList();

                foreach (var row in rows)
                {
                    try
                    {
                        if (!_styles.ContainsKey(row.id))
                            _styles.Add(row.id, new ChatStyle(row.id, row.name, row.required_right));
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Unable to load ChatBubble for ID [" + row.id + "]", ex);
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