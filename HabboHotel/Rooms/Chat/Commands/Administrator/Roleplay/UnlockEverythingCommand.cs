using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using Plus.HabboHotel.Roleplay.Skill;
using System;
using System.Linq;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay
{
    internal class UnlockEverythingCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_unlockall";

        public string Parameters => "%skins|weapons|skills%";

        public string Description => "Unlock all skins, weapons, or skills for yourself.";

        // Staff setup tool — unlocking things changes nothing about the RP state.
        public bool UsableWhileDead => true;

        public bool UsableWhileCuffed => true;

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || @params.Length < 2) {
                session?.SendWhisper("Use :unlockall skins, :unlockall weapons, or :unlockall skills.", 1);
                return;
            }

            string mode = @params[1].ToLower();
            switch (mode) {
                case "skins":
                    UnlockSkins(session);
                    return;
                case "weapons":
                    UnlockWeapons(session);
                    return;
                case "skills":
                    UnlockSkills(session);
                    return;
                default:
                    session.SendWhisper("Use :unlockall skins, :unlockall weapons, or :unlockall skills.", 1);
                    return;
            }
        }

        private static void UnlockSkins(GameClient session)
        {
            UserRpWeaponSkins userSkins = session.GetHabbo().GetRpWeaponSkins();
            int added = 0;

            foreach (WeaponSkin skin in PlusEnvironment.GetWeaponManager().GetSkins()) {
                if (userSkins.Skins.Any(x => x.SkinId == skin.Id))
                    continue;

                // Unsaved skins must carry a negative placeholder id — SaveRpWeaponSkins treats any
                // id > 0 as an existing row to update. Min() over already-persisted (positive) ids
                // would hand out positive ids that collide with, or point at, rows we never inserted.
                int nextId = userSkins.Skins.Any() ? Math.Min(0, userSkins.Skins.Min(x => x.Id)) - 1 : -1;
                userSkins.AddSkin(new UserWeaponSkin(nextId, session.GetHabbo().Id, skin.Id, false, skin));
                added++;
            }

            session.GetHabbo().SaveRpWeaponSkins();
            session.SendWhisper("Unlocked " + added + " weapon skins.", 1);
        }

        private static void UnlockWeapons(GameClient session)
        {
            UserRpWeapons userWeapons = session.GetHabbo().GetRpWeapons();
            int added = 0;

            foreach (Weapon weapon in PlusEnvironment.GetWeaponManager().GetWeapons().Where(x => x.Id != 0)) {
                if (userWeapons.GetWeaponsByWeaponId(weapon.Id).Any())
                    continue;

                if (userWeapons.AddWeapon(weapon.Id) != null)
                    added++;
            }

            session.GetHabbo().SaveRpWeapons();
            session.SendWhisper("Unlocked " + added + " weapons.", 1);
        }

        private static void UnlockSkills(GameClient session)
        {
            UserRpSkills skills = session.GetHabbo().GetRpSkills();
            int updated = 0;

            foreach (Skill skill in PlusEnvironment.GetSkillManager().GetSkills()) {
                if (!skills.GrantMaxSkill(skill.Id))
                    continue;

                updated++;
            }

            session.GetHabbo().SaveRpSkills();
            session.SendWhisper("Unlocked or maxed " + updated + " skills.", 1);
        }
    }
}