using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Roleplay.RpItem.Weapon
{
    public static class RpWeaponService
    {
        public static bool Equip(GameClient session, int weaponId, out string error)
        {
            error = null;
            Users.Habbo habbo = session?.GetHabbo();
            Room room = habbo?.CurrentRoom;
            if (habbo == null || room == null) {
                error = "You need to be in a room to do that.";
                return false;
            }

            RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
            if (user == null)
                return false;

            // Staff on shift can never carry a weapon (unequipping to hands is still allowed).
            if (weaponId != 0 && RpEffectService.IsWorkingStaff(habbo)) {
                error = "Staff can't equip weapons while on duty.";
                return false;
            }

            // Neither can a corpse. The overlay already greys weapons out while dead (blockedReason
            // "dead"), so without this the server silently disagreed with its own inventory.
            if (weaponId != 0 && habbo.GetRpStats()?.IsDead == true) {
                error = "You're dead — you can't equip weapons.";
                return false;
            }

            // A cuffed suspect can't arm themselves.
            if (weaponId != 0 && PlusEnvironment.GetPoliceManager().IsCuffed(habbo.Id)) {
                error = "You can't equip weapons while cuffed.";
                return false;
            }

            if (weaponId != 0 && habbo.GetRpStats()?.PassiveMode == true) {
                error = "A pacifist does not fight. Try disabling passive mode, before combat!";
                return false;
            }

            UserRpWeapons weapons = habbo.GetRpWeapons();
            if (weapons == null || !weapons.SetActiveWeaponId(weaponId)) {
                error = "You don't have that weapon.";
                return false;
            }

            // The primary slot holds a weapon OR a primary item — never both.
            if (weaponId != 0)
                habbo.GetRpItems()?.UnequipPrimaryItem(habbo);

            UserWeapon weapon = weapons.GetActiveWeapon();
            UserWeaponSkin equippedSkin = habbo.GetRpWeaponSkins()?.GetEquippedSkinForWeapon(weapon.WeaponId);

            // Apply the resolved RP effect (weapon/skin here, but respects staff/passive priority).
            RpEffectService.Refresh(habbo);

            string message;
            if (weaponId == 0)
                message = "*unequips weapons*";
            else if (equippedSkin != null)
                message = "*equips " + equippedSkin.SkinData.Name + " as their active weapon skin*";
            else
                message = "*equips " + weapon.WeaponData.Name + " as their active weapon*";

            room.SendPacket(new ShoutComposer(user.VirtualId, message, 0, 11, isRpAction: true));
            return true;
        }

        public static void Disarm(Users.Habbo habbo)
        {
            if (habbo == null)
                return;

            bool hadWeapon = (habbo.GetRpWeapons()?.ActiveWeaponId ?? 0) != 0;
            if (hadWeapon)
                habbo.GetRpWeapons().SetActiveWeaponId(0);

            bool hadSuicideVest = habbo.GetRpItems()?.UnequipSuicideVest(habbo) ?? false;

            if (!hadWeapon && !hadSuicideVest)
                return;

            // Clear the now-stale weapon effect and push the updated stats to the client.
            RpEffectService.Refresh(habbo);
            habbo.TrySendUserStatsUpdate(true);

            Room room = habbo.CurrentRoom;
            RoomUser user = room?.GetRoomUserManager()?.GetRoomUserByHabbo(habbo.Id);
            if (room != null && user != null)
                room.SendPacket(new ShoutComposer(user.VirtualId, "*unequips weapons*", 0, 11, isRpAction: true));
        }
    }
}