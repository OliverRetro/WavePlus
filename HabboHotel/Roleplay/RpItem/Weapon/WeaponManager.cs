using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using log4net;
using Plus.Database.Interfaces;
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

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                // Load weapons
                dbClient.SetQuery("SELECT * FROM `rp_weapons`");
                DataTable getWeapons = dbClient.GetTable();

                if (getWeapons != null) {
                    foreach (DataRow row in getWeapons.Rows)
                    {
                        var weapon = new Weapon(
                            Convert.ToInt32(row["id"]),
                            Convert.ToString(row["name"]),
                            Convert.ToString(row["description"]),
                            Convert.ToString(row["hit_message"]),
                            Convert.ToString(row["critical_hit_message"]),
                            Convert.ToInt32(row["minimum_damage"]),
                            Convert.ToInt32(row["maximum_damage"]),
                            Convert.ToInt32(row["critical_chance"]),
                            Convert.ToDouble(row["critical_multiplier"]),
                            Convert.ToInt32(row["effect_id"]),
                            Convert.ToInt32(row["range"]),
                            Convert.ToInt32(row["durability"]),
                            Convert.ToInt32(row["stun_chance"]),
                            Convert.ToString(row["image"])
                        );

                        _weapons.Add(weapon.Id, weapon);
                    }
                }

                dbClient.SetQuery("SELECT * FROM `rp_weapon_skins`");
                DataTable getSkins = dbClient.GetTable();

                if (getSkins != null) {
                    foreach (DataRow row in getSkins.Rows)
                    {
                        WeaponSkin skin = new(
                            Convert.ToInt32(row["id"]),
                            Convert.ToInt32(row["weapon_id"]),
                            Convert.ToString(row["name"]),
                            Convert.ToString(row["hit_message"]),
                            Convert.ToString(row["critical_hit_message"]),
                            Convert.ToInt32(row["base_price"]),
                            Convert.ToInt32(row["rarity"]),
                            Convert.ToInt32(row["effect_id"])
                        );

                        _skins[skin.Id] = skin;
                    }
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
