using System.Collections.Generic;
using System.Linq;

namespace Plus.HabboHotel.Roleplay.RpItem.Weapon
{
    public class UserRpWeaponSkins
    {
        private readonly Dictionary<int, UserWeaponSkin> _skins;

        public UserRpWeaponSkins(IEnumerable<UserWeaponSkin> skins)
        {
            _skins = skins.ToDictionary(x => x.Id, x => x);
        }

        public IEnumerable<UserWeaponSkin> Skins => _skins.Values;

        public bool Dirty { get; private set; }

        public UserWeaponSkin GetSkin(int userSkinId)
        {
            return _skins.TryGetValue(userSkinId, out UserWeaponSkin skin) ? skin : null;
        }

        public void AddSkin(UserWeaponSkin skin)
        {
            if (skin == null || _skins.ContainsKey(skin.Id))
                return;

            _skins[skin.Id] = skin;
            Dirty = true;
        }

        public IEnumerable<UserWeaponSkin> GetSkinsForWeapon(int weaponId)
        {
            return _skins.Values.Where(x => x.SkinData?.WeaponId == weaponId);
        }

        public UserWeaponSkin GetEquippedSkinForWeapon(int weaponId)
        {
            return _skins.Values.FirstOrDefault(x => x.Equipped && x.SkinData?.WeaponId == weaponId);
        }

        public bool ToggleSkin(int userSkinId)
        {
            if (!_skins.TryGetValue(userSkinId, out UserWeaponSkin targetSkin))
                return false;

            foreach (UserWeaponSkin skin in GetSkinsForWeapon(targetSkin.SkinData.WeaponId)) {
                bool shouldEquip = skin.Id == userSkinId && !targetSkin.Equipped;
                if (skin.Equipped == shouldEquip)
                    continue;

                skin.Equipped = shouldEquip;
                Dirty = true;
            }

            return true;
        }

        public void MarkSaved()
        {
            Dirty = false;
        }

        public void RekeySkin(int oldId, int newId)
        {
            if (oldId == newId || !_skins.TryGetValue(oldId, out UserWeaponSkin skin))
                return;

            _skins.Remove(oldId);
            _skins[newId] = skin;
        }
    }
}