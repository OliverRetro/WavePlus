using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using System;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay
{
    internal class EquipWeaponCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_equipweapon";

        public string Parameters => "%weaponId%";

        public string Description => "Equip a weapon for now.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (!int.TryParse(Convert.ToString(@params[1]), out int weaponId)) {
                session.SendWhisper("Please enter a valid ID. You can see these via :tempinventory.");
                return;
            }

            if (!RpWeaponService.Equip(session, weaponId, out string error) && error != null)
                session.SendWhisper(error);
        }
    }
}