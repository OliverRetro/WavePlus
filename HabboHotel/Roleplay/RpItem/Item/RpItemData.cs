using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Plus.HabboHotel.Roleplay.RpItem.Item
{
    public class RpItemData
    {
        public const int TimeUnitMs = 10;

        public const int BaseHungerTickMinutes = 10;

        public int Id { get; }

        public string Name { get; }

        public int HanditemData { get; }

        public int EffectData { get; }

        public int BasePrice { get; }

        public string ImageUrl { get; }

        public int StackLimit { get; }

        public string ItemType { get; }

        public int Rarity { get; }

        public string BubbleText { get; }

        public int BubbleId { get; }

        public string Clothing { get; }

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

        public int MaxUses => GetInt("uses", 1);

        public bool IsNone => string.Equals(ItemType, "none", StringComparison.OrdinalIgnoreCase);

        public bool IsPrimary => string.Equals(ItemType, "primary", StringComparison.OrdinalIgnoreCase);

        public bool IsSecondary =>
            string.Equals(ItemType, "secondary", StringComparison.OrdinalIgnoreCase);

        public bool IsEquippable => IsPrimary || IsSecondary;

        public bool IsConsumable => !IsNone && !IsEquippable;

        public bool UsableWhileDead => GetInt("zombie", 0) > 0;

        public bool UsableWhileCuffed => GetInt("usable_cuffed", 0) > 0;

        public bool IsShield => IsSecondary;

        public int MaxShield => GetInt("shield", 0);

        public bool IsVest => Has("vest");

        public int MaxVestDurability => GetInt("vest_durability", GetInt("shield", 20));

        public int MaxDurability => IsVest ? MaxVestDurability : MaxShield;

        public bool IsSuicideVest => Has("suicide_vest");

        public bool BlockedWhilePassive => Has("passive");

        public int VestReductionMin => GetInt("vest_reduction_min", 1);

        public int VestReductionMax => Math.Max(VestReductionMin, GetInt("vest_reduction_max", 3));

        public int VestFailChance => Math.Clamp(GetInt("vest_fail_chance", 0), 0, 100);

        // cooldown in ms i cba to do summary
        public int CooldownMs => GetInt("cooldown", 0);

        // medkits, bandages again cba w summary
        public bool IsSlowHeal => GetInt("slow_heal", 0) > 0;

        public int ExplosionMin => GetInt("explosion_min", 50);

        public int ExplosionMax => Math.Max(ExplosionMin, GetInt("explosion_max", 100));

        public bool IsStackable => StackLimit > 1 && !IsEquippable;

        public bool Has(string key) => _attributes.ContainsKey(key);

        public double Get(string key, double fallback = 0) =>
            _attributes.TryGetValue(key, out double value) ? value : fallback;

        public int GetInt(string key, int fallback = 0) =>
            _attributes.TryGetValue(key, out double value) ? (int)value : fallback;

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

            try {
                using JsonDocument doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                    return result;

                foreach (JsonProperty prop in doc.RootElement.EnumerateObject()) {
                    if (prop.Value.ValueKind == JsonValueKind.Number && prop.Value.TryGetDouble(out double number))
                        result[prop.Name] = number;
                }
            } catch {
                // Malformed attribute JSON -> treat the item as having no attributes.
            }

            return result;
        }
    }
}