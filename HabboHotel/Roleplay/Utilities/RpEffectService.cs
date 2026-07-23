using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Roleplay.Utilities
{
    public static class RpEffectService
    {
        public const int StaffEffect = 102;
        public const int PassiveEffect = 1057;
        public const int GodModeEffect = 59;
        public const int CuffedEffect = 590;
        public static int GetStaffCorporationId() => int.TryParse(PlusEnvironment.GetSettingsManager().TryGetValue("rp.staff.corporation.id"), out int id) ? id : 0;

        public static bool IsWorkingStaff(Habbo habbo)
        {
            if (habbo == null)
                return false;

            int staffCorp = GetStaffCorporationId();
            return staffCorp > 0 && PlusEnvironment.GetGame().GetShiftManager().IsWorkingFor(habbo.Id, staffCorp);
        }

        public static int Resolve(Habbo habbo)
        {
            if (habbo == null)
                return 0;

            if (PlusEnvironment.GetPoliceManager().IsCuffed(habbo.Id) && !PlusEnvironment.GetJailManager().IsJailed(habbo.Id))
                return CuffedEffect;

            if (IsWorkingStaff(habbo))
                return StaffEffect;

            UserWeapon active = habbo.GetRpWeapons()?.GetActiveWeapon();
            if (active?.WeaponData != null && active.WeaponData.Id != 0) {
                UserWeaponSkin skin = habbo.GetRpWeaponSkins()?.GetEquippedSkinForWeapon(active.WeaponId);
                return skin?.SkinData?.EffectId ?? active.WeaponData.Effect;
            }

            if (habbo.GetRpStats()?.PassiveMode == true)
                return PassiveEffect;

            if (habbo.GetRpStats()?.IsGodMode == true)
                return GodModeEffect;

            return 0;
        }

        public static void Refresh(Habbo habbo)
        {
            if (habbo == null)
                return;

            habbo.Effects()?.ApplyEffect(Resolve(habbo));
        }
    }
}