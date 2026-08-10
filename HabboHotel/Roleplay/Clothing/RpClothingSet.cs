using System.Collections.Generic;

namespace Plus.HabboHotel.Roleplay.Clothing
{
    public sealed class RpClothingSet
    {
        public int Id { get; }
        public int TabId { get; }
        public string SetType { get; }
        public string PartTypesRaw { get; }
        public string ColorRaw { get; }

        public string Name { get; }
        public int Stock { get; set; }
        public int BasePrice { get; }
        public int DiscountPrice { get; }
        public bool Visible { get; }

        public IReadOnlyList<int> PartIds { get; }
        public IReadOnlyList<int> ColorIds { get; }

        public RpClothingSet(int id, int tabId, string setType, string partTypesRaw, string colorRaw,
            string name, int stock, int basePrice, int discountPrice, bool visible)
        {
            Id = id;
            TabId = tabId;
            SetType = setType ?? string.Empty;
            PartTypesRaw = partTypesRaw ?? string.Empty;
            ColorRaw = colorRaw ?? string.Empty;
            Name = name ?? string.Empty;
            Stock = stock;
            BasePrice = basePrice;
            DiscountPrice = discountPrice;
            Visible = visible;

            PartIds = SplitInts(PartTypesRaw, ',');
            ColorIds = SplitInts(ColorRaw, '-');
        }

        public int EffectivePrice => (DiscountPrice > 0 && DiscountPrice < BasePrice) ? DiscountPrice : BasePrice;

        public bool InStock => Stock > 0;

        private static List<int> SplitInts(string raw, char sep)
        {
            List<int> list = new();
            if (string.IsNullOrWhiteSpace(raw))
                return list;

            foreach (string piece in raw.Split(sep)) {
                if (int.TryParse(piece.Trim(), out int value))
                    list.Add(value);
            }

            return list;
        }
    }
}