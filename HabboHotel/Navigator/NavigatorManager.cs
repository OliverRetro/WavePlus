using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using log4net;
using Plus.Database.Interfaces;

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

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `navigator_categories` ORDER BY `id` ASC");
                DataTable table = dbClient.GetTable();

                if (table != null) {
                    foreach (DataRow row in table.Rows)
                    {
                        if (Convert.ToInt32(row["enabled"]) == 1) {
                            if (!_searchResultLists.ContainsKey(Convert.ToInt32(row["id"])))
                                _searchResultLists.Add(Convert.ToInt32(row["id"]), new SearchResultList(Convert.ToInt32(row["id"]), Convert.ToString(row["category"]), Convert.ToString(row["category_identifier"]), Convert.ToString(row["public_name"]), true, -1, Convert.ToInt32(row["required_rank"]), NavigatorViewModeUtility.GetViewModeByString(Convert.ToString(row["view_mode"])), Convert.ToString(row["category_type"]), Convert.ToString(row["search_allowance"]), Convert.ToInt32(row["order_id"])));
                        }
                    }
                }

                dbClient.SetQuery("SELECT `room_id`,`caption`,`description`,`image_url`,`enabled` FROM `navigator_publics` ORDER BY `order_num` ASC");
                DataTable getPublics = dbClient.GetTable();

                if (getPublics != null) {
                    foreach (DataRow row in getPublics.Rows)
                    {
                        if (Convert.ToInt32(row["enabled"]) == 1) {
                            if (!_featuredRooms.ContainsKey(Convert.ToInt32(row["room_id"])))
                                _featuredRooms.Add(Convert.ToInt32(row["room_id"]), new FeaturedRoom(Convert.ToInt32(row["room_id"]), Convert.ToString(row["caption"]), Convert.ToString(row["description"]), Convert.ToString(row["image_url"])));
                        }
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