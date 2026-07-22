using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class SellRoomCommand : IChatCommand
    {
        public string Description => "Allows the user to sell their own room.";

        public string Parameters => "%price%";

        public string PermissionRequired => "command_sell_room";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (!room.CheckRights(session, true))
                return;

            if (@params.Length == 1) {
                session.SendWhisper("Oops, you forgot to choose a price to sell the room for.");
                return;
            }

            if (room.Group != null) {
                session.SendWhisper("Oops, this room has a group. You must delete the group before you can sell the room.");
                return;
            }

            if (!int.TryParse(@params[1], out int price)) {
                session.SendWhisper("Oops, you've entered an invalid integer.");
                return;
            }

            if (price == 0) {
                session.SendWhisper("Oops, you cannot sell a room for 0 credits.");
                return;
            }

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.Rooms.Where(r => r.Id == room.Id).ExecuteUpdate(s => s.SetProperty(r => r.SalePrice, price));
            }

            session.SendNotification("Your room is now up for sale. The the current room visitors have been alerted, any item that belongs to you in this room will be transferred to the new owner once purchased. Other items shall be ejected.");

            foreach (RoomUser user in room.GetRoomUserManager().GetRoomUsers()) {
                if (user == null || user.GetClient() == null)
                    continue;

                user.GetClient().SendWhisper("Attention! This room has been put up for sale, you can buy it now for " + price + " credits! Use the :buyroom command.");
            }
        }
    }
}