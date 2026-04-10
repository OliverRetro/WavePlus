using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorArrowTeleport : IFurniInteractor
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

            if (user.Coordinate == item.Coordinate)
            {
                TriggerInstantTeleport(user, item);
                return;
            }

            if (user.CanWalk)
                user.MoveTo(item.SquareInFront);
        }

        public void OnWiredTrigger(Item item)
        {
        }

        public static void TriggerInstantTeleport(RoomUser user, Item item)
        {
            if (item == null || item.GetRoom() == null || user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                return;

            Room room = item.GetRoom();

            if (!ItemTeleporterFinder.IsTeleLinked(item.Id, room))
            {
                user.UnlockWalking();
                return;
            }

            int linkedTele = ItemTeleporterFinder.GetLinkedTele(item.Id);
            if (linkedTele == 0)
            {
                user.UnlockWalking();
                return;
            }

            int teleRoomId = ItemTeleporterFinder.GetTeleRoomId(linkedTele, room);
            if (teleRoomId == 0)
            {
                user.UnlockWalking();
                return;
            }

            if (teleRoomId == room.RoomId)
            {
                Item targetItem = room.GetRoomItemHandler().GetItem(linkedTele);
                if (targetItem == null)
                {
                    user.UnlockWalking();
                    return;
                }

                room.GetGameMap().TeleportToItem(user, targetItem);
                return;
            }

            user.GetClient().GetHabbo().IsTeleporting = true;
            user.GetClient().GetHabbo().TeleportingRoomId = teleRoomId;
            user.GetClient().GetHabbo().TeleportId = linkedTele;
            user.GetClient().GetHabbo().PrepareRoom(teleRoomId, string.Empty);
        }
    }
}
