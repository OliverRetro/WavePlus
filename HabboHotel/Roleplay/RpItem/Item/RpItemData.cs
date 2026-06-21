using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Plus.HabboHotel.Roleplay.RpItem.Item
{
    /// <summary>
    /// Definition of an RP item (a row in `rp_items`). One instance is shared by every
    /// owned copy of the item, the per-user copy lives in <see cref="UserRpItem"/>.
    /// </summary>
    public class RpItemData
    {
        /// <summary>
        /// Multiplier that converts an attribute time value into real milliseconds.
        /// e.g. consume_length 400 => 4000ms (4s), fastwalk 1000 => 10000ms (10s).
        /// </summary>
        public const int TimeUnitMs = 10;

        /// <summary>
        /// Base time, in minutes, that `hunger_saturation` is multiplied against.
        /// </summary>
        public const int BaseHungerTickMinutes = 10;

        public int Id { get; }
        public string Name { get; }
        public int HanditemData { get; }
        public int EffectData { get; }
        public int BasePrice { get; }
        public string ImageUrl { get; }

        private readonly Dictionary<string, double> _attributes;

        public RpItemData(int id, string name, int handitemData, int effectData, int basePrice, string imageUrl, string attributeJson)
        {
            Id = id;
            Name = name;
            HanditemData = handitemData;
            EffectData = effectData;
            BasePrice = basePrice;
            ImageUrl = imageUrl;
            _attributes = ParseAttributes(attributeJson);
        }

        /// <summary>
        /// How many times an owned copy can be used before it is destroyed. Defaults to 1.
        /// </summary>
        public int MaxUses => GetInt("uses", 1);

        public bool Has(string key) => _attributes.ContainsKey(key);

        public double Get(string key, double fallback = 0) =>
            _attributes.TryGetValue(key, out double value) ? value : fallback;

        public int GetInt(string key, int fallback = 0) =>
            _attributes.TryGetValue(key, out double value) ? (int)value : fallback;

        /// <summary>
        /// Real duration (ms) the temporary handitem/effect stays applied before the cached
        /// state is restored. Read from `consume_length`, falling back to `length`.
        /// </summary>
        public int ConsumptionDurationMs
        {
            get
            {
                double value = Has("consume_length") ? Get("consume_length") : Get("length");
                return (int)(value * TimeUnitMs);
            }
        }

        private static Dictionary<string, double> ParseAttributes(string json)
        {
            var result = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(json))
                return result;

            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                    return result;

                foreach (JsonProperty prop in doc.RootElement.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Number && prop.Value.TryGetDouble(out double number))
                        result[prop.Name] = number;
                }
            }
            catch
            {
                // Malformed attribute JSON -> treat the item as having no attributes.
            }

            return result;
        }
    }
}
