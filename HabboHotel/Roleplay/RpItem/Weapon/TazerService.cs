using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay.Police;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Roleplay.RpItem.Weapon
{
    public static class TazerService
    {
        public static int TazerWeaponId =>
            int.TryParse(PlusEnvironment.GetSettingsManager().TryGetValue("rp.tazer.weapon.id"), out int id) ? id : 0;

        public static bool IsTazer(int weaponId) => weaponId != 0 && weaponId == TazerWeaponId;

        public static bool IsTazer(Weapon weapon) => weapon != null && IsTazer(weapon.Id);

        public static void GiveTazer(Habbo habbo)
        {
            int tazerId = TazerWeaponId;
            if (habbo == null || tazerId <= 0)
                return;

            UserRpWeapons weapons = habbo.GetRpWeapons();
            if (weapons == null)
                return;

            RemoveTazer(habbo);

            UserWeapon issued = weapons.AddWeapon(tazerId);
            if (issued == null)
                return;

            habbo.GetRpItems()?.UnequipPrimaryItem(habbo);
            weapons.SetActiveWeaponId(issued.Id);
            RpEffectService.Refresh(habbo);
        }

        public static void RemoveTazer(Habbo habbo)
        {
            int tazerId = TazerWeaponId;
            UserRpWeapons weapons = habbo?.GetRpWeapons();
            if (weapons == null || tazerId <= 0)
                return;

            foreach (UserWeapon weapon in weapons.GetWeaponsByWeaponId(tazerId).ToList())
                weapons.RemoveWeapon(weapon.Id);

            RpEffectService.Refresh(habbo);
        }

        public static void Recharge(Habbo habbo)
        {
            int tazerId = TazerWeaponId;
            UserRpWeapons weapons = habbo?.GetRpWeapons();
            if (weapons == null || tazerId <= 0)
                return;

            bool changed = false;
            foreach (UserWeapon weapon in weapons.GetWeaponsByWeaponId(tazerId)) {
                int max = weapon.WeaponData?.Durability ?? 0;
                if (weapon.DurabilityLeft < max) {
                    weapon.DurabilityLeft = max;
                    changed = true;
                }
            }

            if (changed)
                weapons.MarkDirty();
        }

        public static void TryRechargeAtStation(Habbo habbo)
        {
            if (habbo == null || TazerWeaponId <= 0 || !PoliceManager.IsOnDutyPolice(habbo))
                return;

            if (!GetStationRooms().Contains(habbo.CurrentRoomId))
                return;

            Recharge(habbo);
            habbo.GetClient()?.SendWhisper("Your tazer has been recharged at the station.", 1);
        }

        private static HashSet<int> GetStationRooms()
        {
            HashSet<int> rooms = [];

            string raw = PlusEnvironment.GetSettingsManager().TryGetValue("rp.police.station.rooms");
            if (!string.IsNullOrWhiteSpace(raw)) {
                foreach (string part in raw.Split(';'))
                    if (int.TryParse(part.Trim(), out int id) && id > 0)
                        rooms.Add(id);
            }

            // Fall back to the police corporation's room.
            int policeCorp = PoliceManager.GetConfiguredGroupId("rp.police.corporation.id");
            if (policeCorp > 0 && PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(policeCorp, out Group group) && group.RoomId > 0)
                rooms.Add(group.RoomId);

            return rooms;
        }
    }
}