using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorOneWayGate : IFurniInteractor
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
                    user.UnlockWalking();
                }

                item.InteractingUser = 0;
            }
        }

        public void OnRemove(GameClient session, Item item)
        {
            item.ExtraData = "0";

            if (item.InteractingUser != 0)
            {
                RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);

                if (user != null)
                {
                    user.ClearMovement(true);
                    user.UnlockWalking();
                }

                item.InteractingUser = 0;
            }
        }

        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
            if (session == null)
                return;

            RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);

            if (item.InteractingUser2 != user.UserId)
                item.InteractingUser2 = user.UserId;

            if (user == null)
            {
                return;
            }

            if (item.GetBaseItem().InteractionType == InteractionType.OneWayGate)
            {
                if (user.Coordinate != item.SquareInFront && user.CanWalk)
                {
                    user.MoveTo(item.SquareInFront);
                    return;
                }

                if (!item.GetRoom().GetGameMap().ValidTile(item.SquareBehind.X, item.SquareBehind.Y) ||
                    !item.GetRoom().GetGameMap().CanWalk(item.SquareBehind.X, item.SquareBehind.Y, false)
                    || !item.GetRoom().GetGameMap().SquareIsOpen(item.SquareBehind.X, item.SquareBehind.Y, false))
                {
                    return;
                }

                if ((user.LastInteraction - PlusEnvironment.GetUnixTimestamp() < 0) && user.InteractingGate &&
                    user.GateId == item.Id)
                {
                    user.InteractingGate = false;
                    user.GateId = 0;
                }


                if (!item.GetRoom().GetGameMap().CanWalk(item.SquareBehind.X, item.SquareBehind.Y, user.AllowOverride))
                {
                    return;
                }

                if (item.InteractingUser == 0)
                {
                    user.InteractingGate = true;
                    user.GateId = item.Id;
                    item.InteractingUser = user.HabboId;

                    user.CanWalk = false;

                    if (user.IsWalking && (user.GoalX != item.SquareInFront.X || user.GoalY != item.SquareInFront.Y))
                    {
                        user.ClearMovement(true);
                    }

                    user.AllowOverride = true;
                    user.MoveTo(item.Coordinate);

                    item.RequestUpdate(4, true);
                }
            }
        }

        public void OnWiredTrigger(Item item)
        {
        }
    }
}