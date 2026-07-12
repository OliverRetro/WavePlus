using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorHopper : IFurniInteractor
    {
        public void OnPlace(GameClient session, Item item)
        {
            item.GetRoom().GetRoomItemHandler().HopperCount++;
            int hopperId = item.Id;
            int hopperRoomId = item.RoomId;
            // NOTE: no EF entity exists for `items_hopper`; raw parameterized SQL through the context.
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                db.Database.ExecuteSqlInterpolated($"INSERT INTO items_hopper (hopper_id, room_id) VALUES ({hopperId}, {hopperRoomId});");
            }

            if (item.InteractingUser != 0) {
                RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);

                if (user != null) {
                    user.ClearMovement(true);
                    user.AllowOverride = false;
                    user.CanWalk = true;
                }

                item.InteractingUser = 0;
            }
        }

        public void OnRemove(GameClient session, Item item)
        {
            item.GetRoom().GetRoomItemHandler().HopperCount--;
            int hid = item.Id;
            int hopperRoomId = item.GetRoom().RoomId;
            // NOTE: no EF entity exists for `items_hopper`; raw parameterized SQL through the context.
            // NOTE: legacy query filters on column `item_id` here though the insert wrote `hopper_id`; preserved as-is.
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                db.Database.ExecuteSqlInterpolated($"DELETE FROM items_hopper WHERE item_id={hid} OR room_id={hopperRoomId} LIMIT 1");
            }

            if (item.InteractingUser != 0) {
                RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);

                user?.UnlockWalking();

                item.InteractingUser = 0;
            }
        }

        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
            if (item == null || item.GetRoom() == null || session == null || session.GetHabbo() == null)
                return;
            RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);

            if (user == null) {
                return;
            }

            // Alright. But is this user in the right position?
            if (user.Coordinate == item.Coordinate || user.Coordinate == item.SquareInFront) {
                // Fine. But is this tele even free?
                if (item.InteractingUser != 0) {
                    return;
                }

                user.TeleDelay = 2;
                item.InteractingUser = user.GetClient().GetHabbo().Id;
            } else if (user.CanWalk) {
                user.MoveTo(item.SquareInFront);
            }
        }

        public void OnWiredTrigger(Item item)
        {
        }
    }
}