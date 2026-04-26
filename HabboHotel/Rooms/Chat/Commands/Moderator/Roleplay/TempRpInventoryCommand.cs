using Plus.Database.Interfaces;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Skill;
using System;
using System.Data;
using System.Text;
using System.Linq;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Roleplay
{
    internal class TempRpInventoryCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_user_info";

        public string Parameters => "%username%";

        public string Description => "View another users RP information.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            StringBuilder habboInfo = new();
            habboInfo.Append("<b>Weapons:</b>\r");
            if (session != null)
            {
                habboInfo.Append("Equipped Weapon: <b>" + session.GetHabbo().GetRpStats().ActiveWeapon().WeaponData.Name + "</b>\r\r");

                habboInfo.Append("<b>Weapons owned:</b>\r");

                foreach (UserWeapon weaponData in session.GetHabbo().GetRpWeapons().Weapons)
                {
                    Weapon weapon = PlusEnvironment.GetWeaponManager().GetWeaponById(weaponData.WeaponId);

                    if (weapon == null)
                        continue;

                    habboInfo.Append($"ID: {weaponData.Id} - {weapon.Name} - Durability: {weaponData.DurabilityLeft}\r");
                }

                habboInfo.Append("\r<b>Weapon skins owned:</b>\r");

                foreach (UserWeaponSkin skinData in session.GetHabbo().GetRpWeaponSkins().Skins)
                {
                    WeaponSkin skin = skinData.SkinData;
                    Weapon baseWeapon = PlusEnvironment.GetWeaponManager().GetWeaponById(skin?.WeaponId ?? 0);

                    if (skin == null)
                        continue;

                    habboInfo.Append($"ID: {skinData.Id} - {skin.Name} - Weapon: {baseWeapon?.Name ?? "Unknown"} - Equipped: {(skinData.Equipped ? "Yes" : "No")}\r");
                }

                habboInfo.Append("\r<b>Other Items:</b>\r");
                habboInfo.Append("Not coded yet.");
            }
            else
            {
                habboInfo.Append("It appears you're offline. This is a bug. Please inform Oliver.\r");
            }

            session.SendNotification(habboInfo.ToString());
        }
    }
}
