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

        /// <summary>How many owned copies stack into a single inventory slot. 1 = never stacks.</summary>
        public int StackLimit { get; }

        /// <summary>Category of the item: "consumable" (default), "shield", etc.</summary>
        public string ItemType { get; }

        private readonly Dictionary<string, double> _attributes;

        public RpItemData(int id, string name, int handitemData, int effectData, int basePrice, string imageUrl, string attributeJson, int stackLimit = 1, string itemType = "consumable")
        {
            Id = id;
            Name = name;
            HanditemData = handitemData;
            EffectData = effectData;
            BasePrice = basePrice;
            ImageUrl = imageUrl;
            StackLimit = stackLimit < 1 ? 1 : stackLimit;
            ItemType = string.IsNullOrWhiteSpace(itemType) ? "consumable" : itemType;
            _attributes = ParseAttributes(attributeJson);
        }

        /// <summary>
        /// How many times an owned copy can be used before it is destroyed. Defaults to 1.
        /// </summary>
        public int MaxUses => GetInt("uses", 1);

        /// <summary>Equips to the primary slot (replaces the weapon while equipped).</summary>
        public bool IsPrimary => string.Equals(ItemType, "primary", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Equips to the secondary slot. NOTE: the legacy "shield" alias was intentionally
        /// removed — shield items are now plain consumables that grant the `shield` stat on
        /// use (see <see cref="UserRpItems.ApplyStatEffects"/>), not equippable vests.
        /// </summary>
        public bool IsSecondary =>
            string.Equals(ItemType, "secondary", StringComparison.OrdinalIgnoreCase);

        /// <summary>Any item that occupies an equipped slot (primary or secondary).</summary>
        public bool IsEquippable => IsPrimary || IsSecondary;

        /// <summary>Back-compat alias: the secondary slot behaves as a shield (durability + shield stat).</summary>
        public bool IsShield => IsSecondary;

        /// <summary>Maximum shield/durability for a secondary (shield) item (from the `shield` attribute).</summary>
        public int MaxShield => GetInt("shield", 0);

        /// <summary>
        /// Whether copies of this item collapse into a stack. Items with their own per-copy
        /// state — durability/equip slots, or a stack limit of 1 — never stack.
        /// </summary>
        public bool IsStackable => StackLimit > 1 && !IsEquippable;

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
