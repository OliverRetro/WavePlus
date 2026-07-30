using System.Collections.Generic;
using System.Linq;
using Plus.Database.EF;

namespace Plus.HabboHotel.Users.Calendar
{
    public sealed class CalendarComponent
    {
        private readonly List<int> _lateBoxes;

        private readonly List<int> _openedBoxes;

        public CalendarComponent()
        {
            _lateBoxes = new List<int>();
            _openedBoxes = new List<int>();
        }

        public bool Init(Habbo player)
        {
            if (_lateBoxes.Count > 0)
                _lateBoxes.Clear();

            if (_openedBoxes.Count > 0)
                _openedBoxes.Clear();

            int playerId = player.Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var boxes = db.user_xmas15_calendars.Where(c => c.UserId == playerId).Select(c => new { c.Status, c.Day }).ToList();
                foreach (var box in boxes) {
                    if (box.Status == 0)
                        _lateBoxes.Add(box.Day);
                    else
                        _openedBoxes.Add(box.Day);
                }
            }

            return true;
        }

        public List<int> GetOpenedBoxes()
        {
            return _openedBoxes;
        }

        public List<int> GetLateBoxes()
        {
            return _lateBoxes;
        }

        public void Dispose()
        {
            _lateBoxes.Clear();
            _openedBoxes.Clear();
        }
    }
}