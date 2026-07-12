using System.Collections.Generic;
using System.Linq;
using log4net;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;

namespace Plus.HabboHotel.Permissions
{
    public sealed class WeaponManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WeaponManager));

        private readonly Dictionary<int, Weapon> _weapons = new();
        private readonly Dictionary<int, WeaponSkin> _skins = new();

        public void Init()
        {
            _weapons.Clear();
            _skins.Clear();

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                // Load weapons
                foreach (var row in db.rp_weapons.AsNoTracking().ToList())
                {
                    var weapon = new Weapon(
                        row.id,
                        row.name,
                        row.description,
                        row.hit_message,
                        row.critical_hit_message,
                        row.minimum_damage,
                        row.maximum_damage,
                        row.critical_chance,
                        row.critical_multiplier,
                        row.effect_id,
                        row.range,
                        row.durability,
                        row.stun_chance,
                        row.image
                    );

                    _weapons.Add(weapon.Id, weapon);
                }

                foreach (var row in db.rp_weapon_skins.AsNoTracking().ToList())
                {
                    WeaponSkin skin = new(
                        row.id,
                        row.weapon_id,
                        row.name,
                        row.hit_message,
                        row.critical_hit_message,
                        row.base_price,
                        row.rarity,
                        row.effect_id
                    );

                    _skins[skin.Id] = skin;
                }

                _weapons.Add(0, new Weapon(0, "Fists", "Basic hand attack", "*punches %username% with their fists, dealing %damage% damage*", "*lands a critical hit on %username%*, dealing %damage% damage.", 1, 3, 0, 1, 0, 1, 999999, 1, "fists.png"));
            }

            Log.Info($"[RP] Loaded {_weapons.Count} Weapons and {_skins.Count} Weapon Skins.");
        }

        public ICollection<Weapon> GetWeapons()
        {
            return _weapons.Values;
        }

        public ICollection<WeaponSkin> GetSkins()
        {
            return _skins.Values;
        }

        public bool TryGetWeapon(int weaponId, out Weapon weapon)
        {
            return _weapons.TryGetValue(weaponId, out weapon);
        }

        public Weapon GetWeaponById(int weaponId)
        {
            _weapons.TryGetValue(weaponId, out var weapon);
            return weapon;
        }

        public WeaponSkin GetSkinById(int skinId)
        {
            _skins.TryGetValue(skinId, out WeaponSkin skin);
            return skin;
        }
    }
}
