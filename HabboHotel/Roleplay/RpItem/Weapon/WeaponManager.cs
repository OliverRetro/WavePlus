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

        public void Init()
        {
            _weapons.Clear();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                // Load weapons
                dbClient.SetQuery("SELECT * FROM `rp_weapons`");
                DataTable getWeapons = dbClient.GetTable();

                if (getWeapons != null)
                {
                    foreach (DataRow row in getWeapons.Rows)
                    {
                        var weapon = new Weapon(
                            Convert.ToInt32(row["id"]),
                            Convert.ToString(row["name"]),
                            Convert.ToString(row["description"]),
                            Convert.ToInt32(row["minimum_damage"]),
                            Convert.ToInt32(row["maximum_damage"]),
                            Convert.ToInt32(row["critical_chance"]),
                            Convert.ToDouble(row["critical_multiplier"]),
                            Convert.ToInt32(row["effect_id"]),
                            Convert.ToInt32(row["range"]),
                            Convert.ToInt32(row["durability"]),
                            Convert.ToInt32(row["stun_chance"])
                        );

                        _weapons.Add(weapon.Id, weapon);
                    }
                }

                _weapons.Add(0, new Weapon(0, "Fists", "Basic hand attack", 1, 3, 0, 1, 0, 1, 999999, 1));
            }

            Log.Info($"[RP] Loaded {_weapons.Count} Weapons.");
        }

        public ICollection<Weapon> GetWeapons()
        {
            return _weapons.Values;
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
    }
}