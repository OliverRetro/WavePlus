using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay
{
    internal class ToggleWeaponSkinCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_weapon_skin";

        public string Parameters => "%userSkinId%";

        public string Description => "Toggle an owned weapon skin.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 2 || !int.TryParse(@params[1], out int userSkinId)) {
                session.SendWhisper("Please enter a valid owned skin ID.", 1);
                return;
            }

            UserRpWeaponSkins weaponSkins = session.GetHabbo().GetRpWeaponSkins();
            UserWeaponSkin userSkin = weaponSkins?.GetSkin(userSkinId);
            if (userSkin == null) {
                session.SendWhisper("You do not own that skin.", 1);
                return;
            }

            if (!weaponSkins.ToggleSkin(userSkinId)) {
                session.SendWhisper("That skin could not be toggled.", 1);
                return;
            }

            bool equipped = userSkin.Equipped;
            UserWeapon activeWeapon = session.GetHabbo().GetRpStats().ActiveWeapon();
            if (activeWeapon != null && activeWeapon.WeaponId == userSkin.SkinData.WeaponId)
                session.GetHabbo().Effects().ApplyEffect(equipped ? userSkin.SkinData.EffectId : activeWeapon.WeaponData.Effect);

            room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, equipped ? "*equips the " + userSkin.SkinData.Name + " skin*" : "*unequips the " + userSkin.SkinData.Name + " skin*", 0, 11, isRpAction: true));
        }
    }
}