namespace Plus.HabboHotel.Roleplay.RpItem.Weapon
{
    public class UserWeaponSkin
    {
        public int Id { get; set; }
        public int UserId { get; }
        public int SkinId { get; }
        public bool Equipped { get; set; }
        public WeaponSkin SkinData { get; }

        public UserWeaponSkin(int id, int userId, int skinId, bool equipped, WeaponSkin skinData)
        {
            Id = id;
            UserId = userId;
            SkinId = skinId;
            Equipped = equipped;
            SkinData = skinData;
        }
    }
}