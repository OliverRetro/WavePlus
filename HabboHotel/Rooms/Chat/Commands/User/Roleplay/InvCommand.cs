using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.RpItem;
using Plus.HabboHotel.Roleplay.RpItem.Item;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay
{
    internal class InvCommand : IChatCommand
    {
        // The overlay shows 10 regular item slots.
        private const int SlotCount = 10;

        public string PermissionRequired => "command_rp_invslot";

        public string Parameters => "%slot%";

        public string Description => "Use the item in the given inventory slot (1-10).";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 2 || !int.TryParse(Convert.ToString(@params[1]), out int slot) || slot < 1 || slot > SlotCount) {
                session.SendWhisper("Usage: :inv <1-" + SlotCount + ">", 1);
                return;
            }

            var habbo = session.GetHabbo();
            UserRpItems items = habbo?.GetRpItems();
            if (items == null)
                return;

            // The slot view mixes (stacked) items and weapons — resolve what's actually there.
            RpInventoryEntry entry = RpInventory.BuildSlotView(habbo, SlotCount)[slot - 1];
            if (entry == null) {
                session.SendWhisper("There's no item in slot " + slot + ".", 1);
                return;
            }

            // A weapon slot swaps the active weapon instead of being consumed.
            if (entry.Kind == "weapon") {
                if (!RpWeaponService.Equip(session, entry.Weapon.Id, out string error) && error != null)
                    session.SendWhisper(error, 1);
                return;
            }

            // A secondary (shield) item equips to the secondary slot.
            if (entry.Kind == "secondary") {
                if (!items.EquipShield(entry.Representative.Id, habbo))
                    session.SendWhisper("You couldn't equip that shield.", 1);
                return;
            }

            // A primary item equips to the primary slot (replacing the weapon).
            if (entry.Kind == "primary") {
                if (!items.EquipPrimaryItem(entry.Representative.Id, habbo))
                    session.SendWhisper("You couldn't equip that item.", 1);
                return;
            }

            if (!items.Use(entry.Representative.Id, habbo))
                session.SendWhisper("You couldn't use that item right now.", 1);
        }
    }
}