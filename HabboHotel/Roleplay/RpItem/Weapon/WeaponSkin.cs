namespace Plus.HabboHotel.Roleplay.RpItem.Weapon
{
    public class WeaponSkin
    {
        public int Id { get; }
        public int WeaponId { get; }
        public string Name { get; }
        public string HitMessage { get; }
        public string CriticalHitMessage { get; }
        public int BasePrice { get; }
        public int Rarity { get; }
        public int EffectId { get; }

        public WeaponSkin(int id, int weaponId, string name, string hitMessage, string criticalHitMessage, int basePrice, int rarity, int effectId)
        {
            Id = id;
            WeaponId = weaponId;
            Name = name;
            HitMessage = hitMessage;
            CriticalHitMessage = criticalHitMessage;
            BasePrice = basePrice;
            Rarity = rarity;
            EffectId = effectId;
        }
    }
}