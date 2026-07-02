using System;
using System.Collections.Generic;
using System.Data;
using log4net;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Catalog.Clothing;
using Plus.HabboHotel.Catalog.Marketplace;
using Plus.HabboHotel.Catalog.Pets;
using Plus.HabboHotel.Catalog.Vouchers;
using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Catalog
{
    public class CatalogManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CatalogManager));

        private readonly MarketplaceManager _marketplace;
        private readonly PetRaceManager _petRaceManager;
        private readonly VoucherManager _voucherManager;
        private readonly ClothingManager _clothingManager;

        private Dictionary<int, int> _itemOffers;
        private readonly Dictionary<int, CatalogPage> _pages;
        private readonly Dictionary<int, CatalogBot> _botPresets;
        private readonly Dictionary<int, Dictionary<int, CatalogItem>> _items;
        private readonly Dictionary<int, CatalogDeal> _deals;
        private readonly Dictionary<int, CatalogPromotion> _promotions;

        public CatalogManager()
        {
            _marketplace = new MarketplaceManager();
            _petRaceManager = new PetRaceManager();

            _voucherManager = new VoucherManager();
            _voucherManager.Init();

            _clothingManager = new ClothingManager();
            _clothingManager.Init();

            _itemOffers = new Dictionary<int, int>();
            _pages = new Dictionary<int, CatalogPage>();
            _botPresets = new Dictionary<int, CatalogBot>();
            _items = new Dictionary<int, Dictionary<int, CatalogItem>>();
            _deals = new Dictionary<int, CatalogDeal>();
            _promotions = new Dictionary<int, CatalogPromotion>();
        }

        public void Init(ItemDataManager itemDataManager)
        {
            if (_pages.Count > 0)
                _pages.Clear();
            if (_botPresets.Count > 0)
                _botPresets.Clear();
            if (_items.Count > 0)
                _items.Clear();
            if (_deals.Count > 0)
                _deals.Clear();
            if (_promotions.Count > 0)
                _promotions.Clear();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`item_id`,`catalog_name`,`cost_credits`,`cost_pixels`,`cost_diamonds`,`amount`,`page_id`,`limited_sells`,`limited_stack`,`offer_active`,`extradata`,`badge`,`offer_id` FROM `catalog_items`");
                DataTable catalogueItems = dbClient.GetTable();

                if (catalogueItems != null) {
                    foreach (DataRow row in catalogueItems.Rows)
                    {
                        if (Convert.ToInt32(row["amount"]) <= 0)
                            continue;

                        int itemId = Convert.ToInt32(row["id"]);
                        int pageId = Convert.ToInt32(row["page_id"]);
                        int baseId = Convert.ToInt32(row["item_id"]);
                        int offerId = Convert.ToInt32(row["offer_id"]);

                        if (!itemDataManager.GetItem(baseId, out ItemData data)) {
                            Log.Error("Couldn't load Catalog Item " + itemId + ", no furniture record found.");
                            continue;
                        }

                        if (!_items.ContainsKey(pageId))
                            _items[pageId] = new Dictionary<int, CatalogItem>();

                        if (offerId != -1 && !_itemOffers.ContainsKey(offerId))
                            _itemOffers.Add(offerId, pageId);

                        _items[pageId].Add(Convert.ToInt32(row["id"]), new CatalogItem(Convert.ToInt32(row["id"]), Convert.ToInt32(row["item_id"]),
                            data, Convert.ToString(row["catalog_name"]), Convert.ToInt32(row["page_id"]), Convert.ToInt32(row["cost_credits"]), Convert.ToInt32(row["cost_pixels"]), Convert.ToInt32(row["cost_diamonds"]),
                            Convert.ToInt32(row["amount"]), Convert.ToInt32(row["limited_sells"]), Convert.ToInt32(row["limited_stack"]), PlusEnvironment.EnumToBool(row["offer_active"].ToString()),
                            Convert.ToString(row["extradata"]), Convert.ToString(row["badge"]), Convert.ToInt32(row["offer_id"])));
                    }
                }

                dbClient.SetQuery("SELECT `id`, `items`, `name`, `room_id` FROM `catalog_deals`");
                DataTable getDeals = dbClient.GetTable();

                if (getDeals != null) {
                    foreach (DataRow row in getDeals.Rows)
                    {
                        int id = Convert.ToInt32(row["id"]);
                        string items = Convert.ToString(row["items"]);
                        string name = Convert.ToString(row["name"]);
                        int roomId = Convert.ToInt32(row["room_id"]);

                        CatalogDeal deal = new(id, items, name, roomId, itemDataManager);

                        if (!_deals.ContainsKey(id))
                            _deals.Add(deal.Id, deal);
                    }
                }


                dbClient.SetQuery("SELECT `id`,`parent_id`,`caption`,`page_link`,`visible`,`enabled`,`min_rank`,`min_vip`,`icon_image`,`page_layout`,`page_strings_1`,`page_strings_2` FROM `catalog_pages` ORDER BY `order_num`");
                DataTable catalogPages = dbClient.GetTable();

                if (catalogPages != null) {
                    foreach (DataRow row in catalogPages.Rows)
                    {
                        _pages.Add(Convert.ToInt32(row["id"]), new CatalogPage(Convert.ToInt32(row["id"]), Convert.ToInt32(row["parent_id"]), row["enabled"].ToString(), Convert.ToString(row["caption"]),
                            Convert.ToString(row["page_link"]), Convert.ToInt32(row["icon_image"]), Convert.ToInt32(row["min_rank"]), Convert.ToInt32(row["min_vip"]), row["visible"].ToString(), Convert.ToString(row["page_layout"]),
                            Convert.ToString(row["page_strings_1"]), Convert.ToString(row["page_strings_2"]),
                            _items.ContainsKey(Convert.ToInt32(row["id"])) ? _items[Convert.ToInt32(row["id"])] : new Dictionary<int, CatalogItem>(), ref _itemOffers));
                    }
                }

                dbClient.SetQuery("SELECT `id`,`name`,`figure`,`motto`,`gender`,`ai_type` FROM `catalog_bot_presets`");
                DataTable bots = dbClient.GetTable();

                if (bots != null) {
                    foreach (DataRow row in bots.Rows)
                    {
                        _botPresets.Add(Convert.ToInt32(row[0]), new CatalogBot(Convert.ToInt32(row[0]), Convert.ToString(row[1]), Convert.ToString(row[2]), Convert.ToString(row[3]), Convert.ToString(row[4]), Convert.ToString(row[5])));
                    }
                }

                dbClient.SetQuery("SELECT * FROM `catalog_promotions`");
                DataTable getPromotions = dbClient.GetTable();

                if (getPromotions != null) {
                    foreach (DataRow row in getPromotions.Rows)
                    {
                        if (!_promotions.ContainsKey(Convert.ToInt32(row["id"])))
                            _promotions.Add(Convert.ToInt32(row["id"]), new CatalogPromotion(Convert.ToInt32(row["id"]), Convert.ToString(row["title"]), Convert.ToString(row["image"]), Convert.ToInt32(row["unknown"]), Convert.ToString(row["page_link"]), Convert.ToInt32(row["parent_id"])));
                    }
                }

                _petRaceManager.Init();
                _clothingManager.Init();
            }

            Log.Info("Catalog Manager -> LOADED");
        }

        public bool TryGetBot(int itemId, out CatalogBot bot)
        {
            return _botPresets.TryGetValue(itemId, out bot);
        }

        public Dictionary<int, int> ItemOffers => _itemOffers;

        public bool TryGetPage(int pageId, out CatalogPage page)
        {
            return _pages.TryGetValue(pageId, out page);
        }

        public bool TryGetDeal(int dealId, out CatalogDeal deal)
        {
            return _deals.TryGetValue(dealId, out deal);
        }

        public ICollection<CatalogPage> GetPages()
        {
            return _pages.Values;
        }

        public ICollection<CatalogPromotion> GetPromotions()
        {
            return _promotions.Values;
        }

        public MarketplaceManager GetMarketplace()
        {
            return _marketplace;
        }

        public PetRaceManager GetPetRaceManager()
        {
            return _petRaceManager;
        }

        public VoucherManager GetVoucherManager()
        {
            return _voucherManager;
        }

        public ClothingManager GetClothingManager()
        {
            return _clothingManager;
        }
    }
}