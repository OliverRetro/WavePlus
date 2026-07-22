namespace Plus.HabboHotel.Roleplay.RpItem.Weapon
{
    public class UserWeapon
    {
        public int Id { get; set; }
        public int UserId { get; }
        public int WeaponId { get; }
        public int DurabilityLeft { get; set; }
        public Weapon WeaponData { get; }
        public bool IsDefault { get; }

        // Inventory grid position (0-based) in the shared item/weapon grid. -1 = unassigned.
        public int Slot { get; set; } = -1;

        public UserWeapon(int id, int userId, int weaponId, int durabilityLeft, Weapon weaponData, bool isDefault = false, int slot = -1)
        {
            Id = id;
            UserId = userId;
            WeaponId = weaponId;
            DurabilityLeft = durabilityLeft;
            WeaponData = weaponData;
            IsDefault = isDefault;
            Slot = slot;
        }
    }
}