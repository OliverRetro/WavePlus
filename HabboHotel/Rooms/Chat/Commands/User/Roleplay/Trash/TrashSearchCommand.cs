using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Roleplay.Trash;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Commands;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Trash
{
    internal class TrashSearchCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_use_item";
        public string Parameters => "";
        public string Description => "Search the nearest trashbin for RP items or credits.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room == null)
                return;

            RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
                return;

            Item nearest = null;
            int nearestDistance = int.MaxValue;

            foreach (Item item in room.GetRoomItemHandler().GetFloor)
            {
                if (!TrashSearchManager.IsTrashBin(item))
                    continue;

                int distance = Math.Abs(item.GetX - user.X) + Math.Abs(item.GetY - user.Y);
                if (distance >= nearestDistance)
                    continue;

                nearest = item;
                nearestDistance = distance;
            }

            if (nearest == null) {
                session.SendWhisper("There are no searchable trashbins in this room.", 1);
                return;
            }

            PlusEnvironment.GetTrashSearchManager()?.StartSearch(session, nearest);
        }
    }
}
