using System;
using System.Linq;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.RpItem;
using Plus.HabboHotel.Roleplay.RpItem.Item;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay
{
    internal class DisbandItemCommand : IChatCommand
    {
        private const int SlotCount = 10;

        public string PermissionRequired => "command_rp_disband_item";

        public string Parameters => "%slot%";

        public string Description => "Destroy the item/stack in the given inventory slot (1-10).";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 2 || !int.TryParse(Convert.ToString(@params[1]), out int slot) || slot < 1 || slot > SlotCount) {
                session.SendWhisper("Usage: :disbanditem <1-" + SlotCount + ">", 1);
                return;
            }

            var habbo = session.GetHabbo();
            UserRpItems items = habbo?.GetRpItems();
            if (items == null)
                return;

            RpInventoryEntry entry = RpInventory.BuildSlotView(habbo, SlotCount)[slot - 1];
            if (entry == null) {
                session.SendWhisper("There's nothing in slot " + slot + ".", 1);
                return;
            }

            if (entry.Kind == "weapon") {
                habbo.GetRpWeapons()?.RemoveWeapon(entry.Weapon.Id);
                habbo.SaveRpWeapons();
                session.SendWhisper($"Destroyed {entry.Weapon.WeaponData?.Name ?? "weapon"}.", 1);
                return;
            }

            string name = entry.Representative?.ItemData?.Name ?? "item";
            int count = entry.Count;
            foreach (UserRpItem item in entry.Stack.ToList())
                items.Remove(item);

            habbo.SaveRpItems();
            session.SendWhisper($"Destroyed {count}x {name}.", 1);
        }
    }
}