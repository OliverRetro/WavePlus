using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using System;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay
{
    internal class EquipWeaponCommand : IChatCommand
    {
        public string PermissionRequired => "command_carry";

        public string Parameters => "%weaponId%";

        public string Description => "Equip a weapon for now.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (!int.TryParse(Convert.ToString(@params[1]), out int weaponId))
            {
                session.SendWhisper("Please enter a valid ID. You can see these via :tempinventory.");
                return;
            }

            RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
            {
                return;
            }

            if(session.GetHabbo().GetRpWeapons() == null || !session.GetHabbo().GetRpWeapons().SetActiveWeaponId(weaponId))
            {
                session.SendWhisper("Somethings wrong. Sorry bout that");
                return;
            }

            if (session.GetHabbo().GetRpStats().PassiveMode)
            {
                session.SendWhisper("A pacifist does not fight. Try disabling passive mode, before combat!");
                return;
            }

            // set the weapon as active, this will also check if the user has the weapon in their inventory and if they are able to equip it.
            session.GetHabbo().GetRpWeapons().SetActiveWeaponId(weaponId);

            // use the equipped weapon, this will apply the weapon's effects to the user and make them able to use it in combat.
            UserWeapon weapon = session.GetHabbo().GetRpWeapons().GetActiveWeapon();
            UserWeaponSkin equippedSkin = session.GetHabbo().GetRpWeaponSkins()?.GetEquippedSkinForWeapon(weapon.WeaponId);
            session.GetHabbo().Effects().ApplyEffect(equippedSkin?.SkinData?.EffectId ?? weapon.WeaponData.Effect);

            //if its hands we fuck it all
            if (weaponId == 0)
            {
                room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*Unequips weapons*", 0, 11));
            }
            else if (equippedSkin != null)
            {
                room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*Equips " + equippedSkin.SkinData.Name + " as their active weapon skin*", 0, 11));
            }
            else
            {
                room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*Equips " + weapon.WeaponData.Name + " as their active weapon*", 0, 11));
            }
        }
    }
}