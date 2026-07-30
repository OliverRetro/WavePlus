using System;
using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.Permissions;

namespace Plus.HabboHotel.Roleplay.RpItem.Weapon
{
    public class UserRpWeapons
    {
        private readonly Dictionary<int, UserWeapon> _weapons;
        private readonly WeaponManager _weaponManager;
        private readonly int _userId;

        private readonly HashSet<int> _removedIds = new();

        public UserRpWeapons(int userId, WeaponManager weaponManager, IEnumerable<UserWeapon> weapons)
        {
            _userId = userId;
            _weaponManager = weaponManager;
            _weapons = weapons.ToDictionary(x => x.Id, x => x);
        }

        public IEnumerable<UserWeapon> Weapons => _weapons.Values;

        public int ActiveWeaponId { get; private set; }

        public bool Dirty { get; private set; }

        public UserWeapon GetWeapon(int userWeaponId)
        {
            return _weapons.TryGetValue(userWeaponId, out UserWeapon weapon) ? weapon : null;
        }

        public IEnumerable<UserWeapon> GetWeaponsByWeaponId(int weaponId)
        {
            return _weapons.Values.Where(x => x.WeaponId == weaponId);
        }

        public UserWeapon GetActiveWeapon()
        {
            return GetWeapon(ActiveWeaponId) ?? CreateDefaultWeapon();
        }

        public bool SetActiveWeaponId(int userWeaponId)
        {
            if (userWeaponId == 0) {
                ActiveWeaponId = 0;
                return true;
            }

            if (!_weapons.ContainsKey(userWeaponId)) {
                return false;
            }

            ActiveWeaponId = userWeaponId;
            return true;
        }

        public UserWeapon AddWeapon(int baseWeaponId)
        {
            if (!_weaponManager.TryGetWeapon(baseWeaponId, out Weapon weapon))
                return null;

            int nextId = _weapons.Count == 0 ? -1 : Math.Min(0, _weapons.Keys.Min()) - 1;
            UserWeapon userWeapon = new(nextId, _userId, baseWeaponId, weapon.Durability, weapon);
            _weapons[userWeapon.Id] = userWeapon;
            MarkDirty();
            return userWeapon;
        }

        public void SetSlot(int userWeaponId, int slot)
        {
            if (!_weapons.TryGetValue(userWeaponId, out UserWeapon weapon) || weapon.Slot == slot)
                return;

            weapon.Slot = slot;
            MarkDirty();
        }

        public IEnumerable<int> RemovedIds => _removedIds;

        public void RemoveWeapon(int userWeaponId)
        {
            if (!_weapons.Remove(userWeaponId, out UserWeapon weapon))
                return;

            if (weapon.Id == ActiveWeaponId)
                ActiveWeaponId = 0;

            if (userWeaponId > 0)
                _removedIds.Add(userWeaponId);

            MarkDirty();
        }

        public void MarkDirty()
        {
            Dirty = true;
        }

        public void MarkSaved()
        {
            Dirty = false;
            _removedIds.Clear();
        }

        public void RekeyWeapon(int oldId, int newId)
        {
            if (oldId == newId || !_weapons.TryGetValue(oldId, out UserWeapon weapon))
                return;

            _weapons.Remove(oldId);
            _weapons[newId] = weapon;
        }

        private UserWeapon CreateDefaultWeapon()
        {
            Weapon defaultWeapon = _weaponManager.GetWeaponById(0);
            return new UserWeapon(0, _userId, defaultWeapon?.Id ?? 0, defaultWeapon?.Durability ?? 999999, defaultWeapon, true);
        }
    }
}