using System;
using System.Collections.Generic;
using System.Linq;
using Plus.Database.EF;
using log4net;
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

            using WavePlusContext db = PlusEnvironment.GetDbContext();

            foreach (var row in db.catalog_items.Select(c => new { c.id, c.item_id, c.catalog_name, c.cost_credits, c.cost_pixels, c.cost_diamonds, c.amount, c.page_id, c.limited_sells, c.limited_stack, c.offer_active, c.extradata, c.badge, c.offer_id }).ToList())
            {
                if (row.amount <= 0)
                    continue;

                int itemId = row.id;
                int pageId = row.page_id;
                int baseId = Convert.ToInt32(row.item_id);
                int offerId = row.offer_id;

                if (!itemDataManager.GetItem(baseId, out ItemData data)) {
                    Log.Error("Couldn't load Catalog Item " + itemId + ", no furniture record found.");
                    continue;
                }

                if (!_items.ContainsKey(pageId))
                    _items[pageId] = new Dictionary<int, CatalogItem>();

                if (offerId != -1 && !_itemOffers.ContainsKey(offerId))
                    _itemOffers.Add(offerId, pageId);

                _items[pageId].Add(row.id, new CatalogItem(row.id, Convert.ToInt32(row.item_id),
                    data, row.catalog_name, row.page_id, row.cost_credits, row.cost_pixels, row.cost_diamonds,
                    row.amount, row.limited_sells, row.limited_stack, PlusEnvironment.EnumToBool(row.offer_active),
                    row.extradata, row.badge, row.offer_id));
            }

            foreach (var row in db.catalog_deals.Select(d => new { d.id, d.items, d.name, d.room_id }).ToList())
            {
                CatalogDeal deal = new(row.id, row.items, row.name, row.room_id, itemDataManager);

                if (!_deals.ContainsKey(row.id))
                    _deals.Add(deal.Id, deal);
            }

            foreach (var row in db.catalog_pages.OrderBy(p => p.order_num).Select(p => new { p.id, p.parent_id, p.caption, p.page_link, p.visible, p.enabled, p.min_rank, p.min_vip, p.icon_image, p.page_layout, p.page_strings_1, p.page_strings_2 }).ToList())
            {
                _pages.Add(row.id, new CatalogPage(row.id, row.parent_id, row.enabled, row.caption,
                    row.page_link, row.icon_image, (int)row.min_rank, row.min_vip, row.visible, row.page_layout,
                    row.page_strings_1, row.page_strings_2,
                    _items.ContainsKey(row.id) ? _items[row.id] : new Dictionary<int, CatalogItem>(), ref _itemOffers));
            }

            foreach (var row in db.catalog_bot_presets.Select(b => new { b.id, b.name, b.figure, b.motto, b.gender, b.ai_type }).ToList())
                _botPresets.Add(row.id, new CatalogBot(row.id, row.name, row.figure, row.motto, row.gender, row.ai_type));

            foreach (var row in db.catalog_promotions.Select(p => new { p.id, p.title, p.image, p.unknown, p.page_link, p.parent_id }).ToList())
            {
                if (!_promotions.ContainsKey(row.id))
                    _promotions.Add(row.id, new CatalogPromotion(row.id, row.title, row.image, row.unknown ?? 0, row.page_link, row.parent_id ?? 0));
            }

            _petRaceManager.Init();
            _clothingManager.Init();

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