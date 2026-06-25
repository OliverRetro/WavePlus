using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.RpItem.Item;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay
{
    /// <summary>
    /// `:inv N` — use the item shown in inventory slot N (1-10), matching the overlay grid.
    /// </summary>
    internal class InvCommand : IChatCommand
    {
        // The overlay shows 10 regular item slots.
        private const int SlotCount = 10;

        public string PermissionRequired => "command_rp_use_item";

        public string Parameters => "%slot%";

        public string Description => "Use the item in the given inventory slot (1-10).";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 2 || !int.TryParse(Convert.ToString(@params[1]), out int slot) || slot < 1 || slot > SlotCount)
            {
                session.SendWhisper("Usage: :inv <1-" + SlotCount + ">", 1);
                return;
            }

            UserRpItems items = session.GetHabbo()?.GetRpItems();
            UserRpItem item = items?.GetItemAtSlot(slot - 1, SlotCount);
            if (item == null)
            {
                session.SendWhisper("There's no item in slot " + slot + ".", 1);
                return;
            }

            if (!items.Use(item.Id, session.GetHabbo()))
                session.SendWhisper("You couldn't use that item right now.", 1);
        }
    }
}
