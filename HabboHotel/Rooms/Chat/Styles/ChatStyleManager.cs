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

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var rows = db.RoomChatStyles.Select(s => new { s.Id, s.Name, s.RequiredRight, s.Url, s.FontColor }).ToList();

                foreach (var row in rows) {
                    try {
                        if (!_styles.ContainsKey(row.Id))
                            _styles.Add(row.Id, new ChatStyle(row.Id, row.Name, row.RequiredRight, row.Url, row.FontColor));
                    } catch (Exception ex) {
                        Log.Error("Unable to load ChatBubble for ID [" + row.Id + "]", ex);
                    }
                }
            }

            Log.Info("Loaded " + _styles.Count + " chat styles.");
        }

        public bool TryGetStyle(int id, out ChatStyle style)
        {
            return _styles.TryGetValue(id, out style);
        }

        public ICollection<ChatStyle> GetStyles()
        {
            return _styles.Values;
        }
    }
}