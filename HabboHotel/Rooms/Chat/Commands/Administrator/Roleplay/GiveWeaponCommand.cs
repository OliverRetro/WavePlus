using System;
using System.Linq;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Permissions;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay
{
    internal class GiveWeaponCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_give_weapon";

        public string Parameters => "%username% %weapon name%";

        public string Description => "Give an RP weapon (by name) to yourself or another user.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 3) {
                session.SendWhisper("Usage: :giveweapon <username> <weapon name>", 1);
                return;
            }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);
            if (targetClient?.GetHabbo() == null) {
                session.SendWhisper("That user could not be found.", 1);
                return;
            }

            // Everything after the username is the weapon name — fuse it back together so names
            // with spaces ("Combat Knife") work.
            string weaponName = string.Join(" ", @params.Skip(2)).Trim();
            if (string.IsNullOrWhiteSpace(weaponName)) {
                session.SendWhisper("Please enter a weapon name.", 1);
                return;
            }

            WeaponManager weaponManager = PlusEnvironment.GetWeaponManager();
            Weapon weapon = weaponManager.GetWeapons()
                .FirstOrDefault(w => w.Id != 0 && string.Equals(w.Name, weaponName, StringComparison.OrdinalIgnoreCase));

            if (weapon == null) {
                session.SendWhisper($"No weapon named \"{weaponName}\" exists.", 1);
                return;
            }

            var targetHabbo = targetClient.GetHabbo();
            UserWeapon given = targetHabbo.GetRpWeapons()?.AddWeapon(weapon.Id);
            if (given == null) {
                session.SendWhisper("That weapon could not be given.", 1);
                return;
            }

            targetHabbo.SaveRpWeapons();

            // Live-refresh the recipient's inventory panel if they have it open.
            Plus.Communication.Packets.Incoming.WebOverlayCallbackEvent.RefreshInventory(targetClient);

            session.SendWhisper($"Gave {weapon.Name} to {targetHabbo.Username}.", 1);
        }
    }
}