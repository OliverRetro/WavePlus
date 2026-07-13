using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorTeleport : IFurniInteractor
    {
        public void OnPlace(GameClient session, Item item)
        {
            item.ExtraData = "0";

            if (item.InteractingUser != 0)
            {
                RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);

                if (user != null)
                {
                    user.ClearMovement(true);
                    user.AllowOverride = false;
                    user.CanWalk = true;
                }

                item.InteractingUser = 0;
            }

            if (item.InteractingUser2 != 0)
            {
                RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser2);

                if (user != null)
                {
                    user.ClearMovement(true);
                    user.AllowOverride = false;
                    user.CanWalk = true;
                }

                item.InteractingUser2 = 0;
            }
        }

        public void OnRemove(GameClient session, Item item)
        {
            item.ExtraData = "0";

            if (item.InteractingUser != 0)
            {
                RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);

                user?.UnlockWalking();

                item.InteractingUser = 0;
            }

            if (item.InteractingUser2 != 0)
            {
                RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser2);

                user?.UnlockWalking();

                item.InteractingUser2 = 0;
            }
        }

        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
            if (item == null || item.GetRoom() == null || session == null || session.GetHabbo() == null)
                return;

            RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
                return;

            user.LastInteraction = PlusEnvironment.GetUnixTimestamp();

            // Alright. But is this user in the right position?
            if (user.Coordinate == item.Coordinate || user.Coordinate == item.SquareInFront)
            {
                // Fine. But is this tele even free?
                if (item.InteractingUser != 0)
                {
                    return;
                }

                if (!user.CanWalk || session.GetHabbo().IsTeleporting || session.GetHabbo().TeleportId != 0 ||
                    (user.LastInteraction + 2) - PlusEnvironment.GetUnixTimestamp() < 0)
                    return;

                user.TeleDelay = 2;

                item.InteractingUser = user.GetClient().GetHabbo().Id;
            }
            else if (user.CanWalk)
            {
                user.MoveTo(item.SquareInFront);
            }
        }

        public void OnWiredTrigger(Item item)
        {
        }
    }
}