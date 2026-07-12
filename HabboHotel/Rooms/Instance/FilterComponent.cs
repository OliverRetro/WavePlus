using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.Database.EF.Entities;

namespace Plus.HabboHotel.Rooms.Instance
{
    public class FilterComponent
    {
        private Room _instance;

        public FilterComponent(Room instance)
        {
            if (instance == null)
                return;

            _instance = instance;
        }

        public bool AddFilter(string word)
        {
            if (_instance.WordFilterList.Contains(word))
                return false;

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                db.room_filters.Add(new room_filter { room_id = _instance.Id, word = word });
                db.SaveChanges();
            }

            _instance.WordFilterList.Add(word);
            return true;
        }

        public bool RemoveFilter(string word)
        {
            if (!_instance.WordFilterList.Contains(word))
                return false;

            int rid = _instance.Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                db.room_filters.Where(f => f.room_id == rid && f.word == word).ExecuteDelete();
            }

            _instance.WordFilterList.Remove(word);
            return true;
        }

        public string CheckMessage(string message)
        {
            foreach (string filter in _instance.WordFilterList)
            {
                if (message.ToLower().Contains(filter) || message == filter)
                    message = Regex.Replace(message, filter, "Bobba", RegexOptions.IgnoreCase);
                else
                    continue;
            }

            return message.TrimEnd(' ');
        }

        public void Cleanup()
        {
            _instance = null;
        }
    }
}
