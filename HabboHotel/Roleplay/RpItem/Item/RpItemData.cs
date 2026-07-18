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

        /// <summary>Item name</summary>
        public string Name { get; }

        /// <summary>Momentary handitem given when used</summary>
        public int HanditemData { get; }

        /// <summary>Momentary effect given when used</summary>
        public int EffectData { get; }

        /// <summary>Reference price for corporations</summary>
        public int BasePrice { get; }

        /// <summary>Image loaded on UI</summary>
        public string ImageUrl { get; }

        /// <summary>How many owned copies stack into a single inventory slot. 1 = never stacks.</summary>
        public int StackLimit { get; }

        /// <summary>Category of the item: "consumable" (default), "shield", etc.</summary>
        public string ItemType { get; }

        /// <summary>Rarity 1-5 (1 common .. 5 legendary) driving the inventory glow.</summary>
        public int Rarity { get; }

        /// <summary>Shout text executed when the item is consumed (empty = none).</summary>
        public string BubbleText { get; }

        /// <summary>Chat-bubble style id used for the consume shout.</summary>
        public int BubbleId { get; }

        /// <summary>Figure override applied while equipped (shift-costume format). Empty = none.</summary>
        public string Clothing { get; }

        /// <summary>Bool for clothes/summary>
        public bool HasClothing => !string.IsNullOrWhiteSpace(Clothing);

        private readonly Dictionary<string, double> _attributes;

        public RpItemData(int id, string name, int handitemData, int effectData, int basePrice, string imageUrl, string attributeJson, int stackLimit = 1, string itemType = "consumable", int rarity = 1, string bubble = "", string clothing = "")
        {
            Clothing = clothing ?? string.Empty;
            Id = id;
            Name = name;
            HanditemData = handitemData;
            EffectData = effectData;
            BasePrice = basePrice;
            ImageUrl = imageUrl;
            StackLimit = stackLimit < 1 ? 1 : stackLimit;
            ItemType = string.IsNullOrWhiteSpace(itemType) ? "consumable" : itemType;
            Rarity = rarity < 1 ? 1 : (rarity > 5 ? 5 : rarity);
            (BubbleText, BubbleId) = ParseBubble(bubble);
            _attributes = ParseAttributes(attributeJson);
        }

        public bool HasBubble => !string.IsNullOrWhiteSpace(BubbleText);

        private static (string text, int bubbleId) ParseBubble(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return (string.Empty, 0);

            int sep = raw.LastIndexOf(';');
            if (sep < 0)
                return (raw.Trim(), 0);

            string text = raw.Substring(0, sep).Trim();
            string idPart = raw.Substring(sep + 1).Trim();
            return (text, int.TryParse(idPart, out int bubbleId) ? bubbleId : 0);
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

        /// <summary>A body-armour vest: a secondary item that flat-reduces incoming non-fist damage.</summary>
        public bool IsVest => Has("vest");

        /// <summary>
        /// Hits a standard vest can absorb before it breaks. Read from `vest_durability`, then the
        /// legacy `shield` attribute, defaulting to 20 so a vest always has durability to spend.
        /// </summary>
        public int MaxVestDurability => GetInt("vest_durability", GetInt("shield", 20));

        /// <summary>Unified max durability for the item's inventory bar (vest hits or shield pool).</summary>
        public int MaxDurability => IsVest ? MaxVestDurability : MaxShield;

        /// <summary>A suicide vest: a secondary item that detonates for area damage when used.</summary>
        public bool IsSuicideVest => Has("suicide_vest");

        /// <summary>
        /// True when this item can't be used while the owner is in passive mode (any item whose
        /// attribute JSON carries a "passive" flag — e.g. aggressive items like the suicide vest).
        /// </summary>
        public bool BlockedWhilePassive => Has("passive");

        /// <summary>Minimum flat damage a standard vest removes from a non-fist hit.</summary>
        public int VestReductionMin => GetInt("vest_reduction_min", 1);

        /// <summary>Maximum flat damage a standard vest removes from a non-fist hit.</summary>
        public int VestReductionMax => Math.Max(VestReductionMin, GetInt("vest_reduction_max", 3));

        /// <summary>Percent chance (0-100) a standard vest fails to reduce a hit but still loses durability.</summary>
        public int VestFailChance => Math.Clamp(GetInt("vest_fail_chance", 0), 0, 100);

        /// <summary>Minimum explosion damage from a suicide vest.</summary>
        public int ExplosionMin => GetInt("explosion_min", 50);

        /// <summary>Maximum explosion damage from a suicide vest.</summary>
        public int ExplosionMax => Math.Max(ExplosionMin, GetInt("explosion_max", 100));

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
