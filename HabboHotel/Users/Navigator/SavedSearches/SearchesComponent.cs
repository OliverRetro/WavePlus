using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Plus.Database.EF;

namespace Plus.HabboHotel.Users.Navigator.SavedSearches
{
    public class SearchesComponent
    {
        private readonly ConcurrentDictionary<int, SavedSearch> _savedSearches;

        public SearchesComponent()
        {
            _savedSearches = new ConcurrentDictionary<int, SavedSearch>();
        }

        public bool Init(Habbo habbo)
        {
            if (_savedSearches.Count > 0)
                _savedSearches.Clear();

            int userId = habbo.Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var searches = db.UserSavedSearches
                    .Where(s => s.UserId == userId)
                    .Select(s => new { s.Id, s.Filter, s.SearchCode })
                    .ToList();

                foreach (var row in searches) {
                    _savedSearches.TryAdd(row.Id, new SavedSearch(row.Id, row.Filter, row.SearchCode));
                }
            }

            return true;
        }

        public ICollection<SavedSearch> Searches => _savedSearches.Values;

        public bool TryAdd(int id, SavedSearch search)
        {
            return _savedSearches.TryAdd(id, search);
        }

        public bool TryRemove(int id, out SavedSearch removed)
        {
            return _savedSearches.TryRemove(id, out removed);
        }
    }
}