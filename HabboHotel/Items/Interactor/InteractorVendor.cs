using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.PathFinding;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorVendor : IFurniInteractor
    {
        public void OnPlace(GameClient session, Item item)
        {
            item.ExtraData = "0";
            item.UpdateNeeded = true;

            if (item.InteractingUser > 0)
            {
                RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);

                if (user != null)
                {
                    user.CanWalk = true;
                }
            }
        }

        public void OnRemove(GameClient session, Item item)
        {
            item.ExtraData = "0";

            if (item.InteractingUser > 0)
            {
                RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);

                if (user != null)
                {
                    user.CanWalk = true;
                }
            }
        }

        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
            if (item.ExtraData != "1" && item.GetBaseItem().VendingIds.Count >= 1 && item.InteractingUser == 0 &&
                session != null)
            {
                RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);

                if (user == null)
                {
                    return;
                }

                if (!Gamemap.TilesTouching(user.X, user.Y, item.GetX, item.GetY))
                {
                    user.MoveTo(item.SquareInFront);
                    return;
                }

                item.InteractingUser = session.GetHabbo().Id;

                user.CanWalk = false;
                user.ClearMovement(true);
                user.SetRot(Rotation.Calculate(user.X, user.Y, item.GetX, item.GetY), false);

                item.RequestUpdate(2, true);

                item.ExtraData = "1";
                item.UpdateState(false, true);
            }
        }

        public void OnWiredTrigger(Item item)
        {
        }
    }
}