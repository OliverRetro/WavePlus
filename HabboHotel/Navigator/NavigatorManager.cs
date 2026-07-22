using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Plus.Database.EF;

namespace Plus.HabboHotel.Navigator
{
    public sealed class NavigatorManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NavigatorManager));

        private readonly Dictionary<int, FeaturedRoom> _featuredRooms;

        private readonly Dictionary<int, TopLevelItem> _topLevelItems;
        private readonly Dictionary<int, SearchResultList> _searchResultLists;

        public NavigatorManager()
        {
            _topLevelItems = new Dictionary<int, TopLevelItem>();
            _searchResultLists = new Dictionary<int, SearchResultList>();

            //Does this need to be dynamic?
            _topLevelItems.Add(1, new TopLevelItem(1, "official_view", "", ""));
            _topLevelItems.Add(2, new TopLevelItem(2, "hotel_view", "", ""));
            _topLevelItems.Add(3, new TopLevelItem(3, "roomads_view", "", ""));
            _topLevelItems.Add(4, new TopLevelItem(4, "myworld_view", "", ""));

            _featuredRooms = new Dictionary<int, FeaturedRoom>();
        }

        public void Init()
        {
            if (_searchResultLists.Count > 0)
                _searchResultLists.Clear();

            if (_featuredRooms.Count > 0)
                _featuredRooms.Clear();

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                foreach (var row in db.NavigatorCategories.OrderBy(c => c.Id).ToList()) {
                    if (Convert.ToInt32(row.Enabled) == 1) {
                        if (!_searchResultLists.ContainsKey(row.Id))
                            _searchResultLists.Add(row.Id, new SearchResultList(row.Id, row.Category, row.CategoryIdentifier, row.PublicName, true, -1, row.RequiredRank, NavigatorViewModeUtility.GetViewModeByString(row.ViewMode), row.CategoryType, row.SearchAllowance, row.OrderId));
                    }
                }

                foreach (var row in db.NavigatorPublics.OrderBy(p => p.OrderNum).Select(p => new { p.RoomId, p.Caption, p.Description, p.ImageUrl, p.Enabled }).ToList()) {
                    if (Convert.ToInt32(row.Enabled) == 1) {
                        if (!_featuredRooms.ContainsKey(row.RoomId))
                            _featuredRooms.Add(row.RoomId, new FeaturedRoom(row.RoomId, row.Caption, row.Description, row.ImageUrl));
                    }
                }
            }

            Log.Info("Navigator -> LOADED");
        }

        public List<SearchResultList> GetCategoriesForSearch(string category)
        {
            IEnumerable<SearchResultList> categories =
                (from cat in _searchResultLists
                 where cat.Value.Category == category
                 orderby cat.Value.OrderId
                 select cat.Value);
            return categories.ToList();
        }

        public ICollection<SearchResultList> GetResultByIdentifier(string category)
        {
            IEnumerable<SearchResultList> categories =
                (from cat in _searchResultLists
                 where cat.Value.CategoryIdentifier == category
                 orderby cat.Value.OrderId
                 select cat.Value);
            return categories.ToList();
        }

        public ICollection<SearchResultList> GetFlatCategories()
        {
            IEnumerable<SearchResultList> categories =
                (from cat in _searchResultLists
                 where cat.Value.CategoryType == NavigatorCategoryType.Category
                 orderby cat.Value.OrderId
                 select cat.Value);
            return categories.ToList();
        }

        public ICollection<SearchResultList> GetEventCategories()
        {
            IEnumerable<SearchResultList> categories =
                (from cat in _searchResultLists
                 where cat.Value.CategoryType == NavigatorCategoryType.PromotionCategory
                 orderby cat.Value.OrderId
                 select cat.Value);
            return categories.ToList();
        }

        public ICollection<TopLevelItem> GetTopLevelItems()
        {
            return _topLevelItems.Values;
        }

        public ICollection<SearchResultList> GetSearchResultLists()
        {
            return _searchResultLists.Values;
        }

        public bool TryGetTopLevelItem(int id, out TopLevelItem topLevelItem)
        {
            return _topLevelItems.TryGetValue(id, out topLevelItem);
        }

        public bool TryGetSearchResultList(int id, out SearchResultList searchResultList)
        {
            return _searchResultLists.TryGetValue(id, out searchResultList);
        }

        public bool TryGetFeaturedRoom(int roomId, out FeaturedRoom publicRoom)
        {
            return _featuredRooms.TryGetValue(roomId, out publicRoom);
        }

        public ICollection<FeaturedRoom> GetFeaturedRooms()
        {
            return _featuredRooms.Values;
        }
    }
}