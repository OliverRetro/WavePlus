using System.Collections.Generic;
using System.Linq;
using Plus.Database.EF;

namespace Plus.HabboHotel.Users.Calendar
{
    /// <summary>
    /// Permissions for a specific Player.
    /// </summary>
    public sealed class CalendarComponent
    {
        /// <summary>
        /// Permission rights are stored here.
        /// </summary>
        private readonly List<int> _lateBoxes;

        private readonly List<int> _openedBoxes;

        public CalendarComponent()
        {
            _lateBoxes = new List<int>();
            _openedBoxes = new List<int>();
        }

        /// <summary>
        /// Initialize the PermissionComponent.
        /// </summary>
        /// <param name="player"></param>
        public bool Init(Habbo player)
        {
            if (_lateBoxes.Count > 0)
                _lateBoxes.Clear();

            if (_openedBoxes.Count > 0)
                _openedBoxes.Clear();

            int playerId = player.Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var boxes = db.user_xmas15_calendars.Where(c => c.user_id == playerId).Select(c => new { c.status, c.day }).ToList();
                foreach (var box in boxes)
                {
                    if (box.status == 0)
                        _lateBoxes.Add(box.day);
                    else
                        _openedBoxes.Add(box.day);
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

        /// <summary>
        /// Dispose of the permissions list.
        /// </summary>
        public void Dispose()
        {
            _lateBoxes.Clear();
            _openedBoxes.Clear();
        }
    }
}