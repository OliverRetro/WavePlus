using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Plus.Database.EF;

namespace Plus.HabboHotel.Rooms.Chat.Filter
{
    public sealed class WordFilterManager
    {
        private readonly List<WordFilter> _filteredWords;

        public WordFilterManager()
        {
            _filteredWords = new List<WordFilter>();
        }

        public void Init()
        {
            if (_filteredWords.Count > 0)
                _filteredWords.Clear();

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var data = db.Wordfilters.Select(w => new { w.Word, w.Replacement, w.Strict, w.Bannable }).ToList();

                foreach (var row in data) {
                    _filteredWords.Add(new WordFilter(row.Word, row.Replacement, PlusEnvironment.EnumToBool(row.Strict), PlusEnvironment.EnumToBool(row.Bannable)));
                }
            }
        }

        public string CheckMessage(string message)
        {
            foreach (WordFilter filter in _filteredWords.ToList()) {
                if (message.ToLower().Contains(filter.Word) && filter.IsStrict || message == filter.Word) {
                    message = Regex.Replace(message, filter.Word, filter.Replacement, RegexOptions.IgnoreCase);
                } else if (message.ToLower().Contains(filter.Word) && !filter.IsStrict || message == filter.Word) {
                    string[] words = message.Split(' ');

                    message = "";
                    foreach (string word in words.ToList()) {
                        if (word.ToLower() == filter.Word)
                            message += filter.Replacement + " ";
                        else
                            message += word + " ";
                    }
                }
            }

            return message.TrimEnd(' ');
        }

        public bool CheckBannedWords(string message)
        {
            message = message.Replace(" ", "").Replace(".", "").Replace("_", "").ToLower();

            foreach (WordFilter filter in _filteredWords.ToList()) {
                if (!filter.IsBannable)
                    continue;

                if (message.Contains(filter.Word))
                    return true;
            }

            return false;
        }

        public bool IsFiltered(string message)
        {
            foreach (WordFilter filter in _filteredWords.ToList()) {
                if (message.Contains(filter.Word))
                    return true;
            }

            return false;
        }
    }
}