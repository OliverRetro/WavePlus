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
            if (_itemOffers.Count > 0)
                _itemOffers.Clear();

            using WavePlusContext db = PlusEnvironment.GetDbContext();

            foreach (var row in db.CatalogItems.Select(c => new { c.Id, c.ItemId, c.CatalogName, c.CostCredits, c.CostPixels, c.CostDiamonds, c.Amount, c.PageId, c.LimitedSells, c.LimitedStack, c.OfferActive, c.Extradata, c.Badge, c.OfferId }).ToList()) {
                if (row.Amount <= 0)
                    continue;

                int itemId = row.Id;
                int pageId = row.PageId;
                int baseId = Convert.ToInt32(row.ItemId);
                int offerId = row.OfferId;

                if (!itemDataManager.GetItem(baseId, out ItemData data)) {
                    Log.Error("Couldn't load Catalog Item " + itemId + ", no furniture record found.");
                    continue;
                }

                if (!_items.ContainsKey(pageId))
                    _items[pageId] = new Dictionary<int, CatalogItem>();

                if (offerId != -1 && !_itemOffers.ContainsKey(offerId))
                    _itemOffers.Add(offerId, pageId);

                _items[pageId].Add(row.Id, new CatalogItem(row.Id, baseId,
                    data, row.CatalogName, row.PageId, row.CostCredits, row.CostPixels, row.CostDiamonds,
                    row.Amount, row.LimitedSells, row.LimitedStack, PlusEnvironment.EnumToBool(row.OfferActive),
                    row.Extradata, row.Badge, offerId));
            }

            foreach (var row in db.CatalogDeals.Select(d => new { d.Id, d.Items, d.Name, d.RoomId }).ToList()) {
                CatalogDeal deal = new(row.Id, row.Items, row.Name, row.RoomId, itemDataManager);

                if (!_deals.ContainsKey(row.Id))
                    _deals.Add(deal.Id, deal);
            }

            foreach (var row in db.CatalogPages.OrderBy(p => p.OrderNum).Select(p => new { p.Id, p.ParentId, p.Caption, p.PageLink, p.Visible, p.Enabled, p.MinRank, p.MinVip, p.IconImage, p.PageLayout, p.PageStrings1, p.PageStrings2 }).ToList()) {
                _pages.Add(row.Id, new CatalogPage(row.Id, row.ParentId, row.Enabled, row.Caption,
                    row.PageLink, row.IconImage, (int)row.MinRank, row.MinVip, row.Visible, row.PageLayout,
                    row.PageStrings1, row.PageStrings2,
                    _items.ContainsKey(row.Id) ? _items[row.Id] : new Dictionary<int, CatalogItem>(), ref _itemOffers));
            }

            foreach (var row in db.CatalogBotPresets.Select(b => new { b.Id, b.Name, b.Figure, b.Motto, b.Gender, b.AiType }).ToList())
                _botPresets.Add(row.Id, new CatalogBot(row.Id, row.Name, row.Figure, row.Motto, row.Gender, row.AiType));

            foreach (var row in db.CatalogPromotions.Select(p => new { p.Id, p.Title, p.Image, p.Unknown, p.PageLink, p.ParentId }).ToList()) {
                if (!_promotions.ContainsKey(row.Id))
                    _promotions.Add(row.Id, new CatalogPromotion(row.Id, row.Title, row.Image, row.Unknown ?? 0, row.PageLink, row.ParentId ?? 0));
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