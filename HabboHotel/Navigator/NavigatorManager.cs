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

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                foreach (var row in db.navigator_categories.OrderBy(c => c.id).ToList())
                {
                    if (Convert.ToInt32(row.enabled) == 1) {
                        if (!_searchResultLists.ContainsKey(row.id))
                            _searchResultLists.Add(row.id, new SearchResultList(row.id, row.category, row.category_identifier, row.public_name, true, -1, row.required_rank, NavigatorViewModeUtility.GetViewModeByString(row.view_mode), row.category_type, row.search_allowance, row.order_id));
                    }
                }

                foreach (var row in db.navigator_publics.OrderBy(p => p.order_num).Select(p => new { p.room_id, p.caption, p.description, p.image_url, p.enabled }).ToList())
                {
                    if (Convert.ToInt32(row.enabled) == 1) {
                        if (!_featuredRooms.ContainsKey(row.room_id))
                            _featuredRooms.Add(row.room_id, new FeaturedRoom(row.room_id, row.caption, row.description, row.image_url));
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