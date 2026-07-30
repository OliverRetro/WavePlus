using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorArrowTeleport : IFurniInteractor
    {
        private static readonly ConcurrentDictionary<int, int> PendingTeleports = new();
        private static readonly ConcurrentDictionary<int, ArrowTeleportArrivalData> PendingArrivals = new();

        private sealed class ArrowTeleportArrivalData
        {
            public int Rotation { get; init; }
            public int HeadRotation { get; init; }
            public int TargetItemId { get; init; }
            public int TargetRoomId { get; init; }
        }

        public void OnPlace(GameClient session, Item item)
        {
            item.ExtraData = "0";

            if (item.InteractingUser != 0) {
                RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);

                if (user != null) {
                    user.ClearMovement(true);
                    user.AllowOverride = false;
                    user.CanWalk = true;
                }

                item.InteractingUser = 0;
            }

            if (item.InteractingUser2 != 0) {
                RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser2);

                if (user != null) {
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

            if (item.InteractingUser != 0) {
                RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);

                user?.UnlockWalking();

                item.InteractingUser = 0;
            }

            if (item.InteractingUser2 != 0) {
                RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser2);

                user?.UnlockWalking();

                item.InteractingUser2 = 0;
            }
        }

        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
            // Clicking the arrow now works like clicking a teleporter: walk to it, or fire it
            // when already standing on it (previously this was a no-op, so clicks on the arrow
            // did nothing at all).
            if (item == null || item.GetRoom() == null || session?.GetHabbo() == null)
                return;

            RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
                return;

            if (user.X == item.GetX && user.Y == item.GetY) {
                BeginDelayedTeleport(user, item);
            } else if (user.CanWalk) {
                user.MoveTo(item.GetX, item.GetY);
            }
        }

        public void OnWiredTrigger(Item item)
        {
        }

        public static void TriggerInstantTeleport(RoomUser user, Item item)
        {
            if (item == null || item.GetRoom() == null || user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                return;

            Room room = item.GetRoom();
            var userHabbo = user.GetClient().GetHabbo();

            if (!ItemTeleporterFinder.IsTeleLinked(item.Id, room)) {
                user.GetClient().Disconnect();
                return;
            }

            int linkedTele = ItemTeleporterFinder.GetLinkedTele(item.Id);
            if (linkedTele == 0) {
                user.GetClient().Disconnect();
                return;
            }

            int teleRoomId = ItemTeleporterFinder.GetTeleRoomId(linkedTele, room);
            if (teleRoomId == 0) {
                user.GetClient().Disconnect();
                return;
            }

            PendingTeleports.TryRemove(userHabbo.Id, out _);
            PendingArrivals[userHabbo.Id] = new ArrowTeleportArrivalData
            {
                Rotation = user.RotBody,
                HeadRotation = user.RotHead,
                TargetItemId = linkedTele,
                TargetRoomId = teleRoomId
            };

            if (teleRoomId == room.RoomId) {
                Item targetItem = room.GetRoomItemHandler().GetItem(linkedTele);
                if (targetItem == null) {
                    user.GetClient().Disconnect();
                    return;
                }

                room.GetGameMap().TeleportToItem(user, targetItem);
                user.SetRot(user.RotBody, false);
                if (PendingArrivals.TryRemove(userHabbo.Id, out ArrowTeleportArrivalData arrivalData)) {
                    user.SetRot(arrivalData.Rotation, false);
                    user.RotHead = arrivalData.HeadRotation;
                }
                return;
            }

            userHabbo.IsTeleporting = true;
            userHabbo.TeleportingRoomId = teleRoomId;
            userHabbo.TeleportId = linkedTele;
            userHabbo.PrepareRoom(teleRoomId, string.Empty);
        }

        public static void BeginDelayedTeleport(RoomUser user, Item item)
        {
            if (user?.GetClient()?.GetHabbo() == null || item == null)
                return;

            int userId = user.GetClient().GetHabbo().Id;
            PendingTeleports[userId] = item.Id;
            _ = Task.Run(() => CompleteDelayedTeleport(userId, item.Id));
        }

        private static void CompleteDelayedTeleport(int userId, int itemId)
        {
            try {
                // A newer walk target cancels the pending teleport (MoveTo → CancelDelayedTeleport).
                if (!PendingTeleports.TryRemove(userId, out int pendingItemId) || pendingItemId != itemId)
                    return;

                GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
                RoomUser user = client?.GetHabbo()?.CurrentRoom?.GetRoomUserManager().GetRoomUserByHabbo(userId);
                if (user == null)
                    return;

                Item item = client.GetHabbo().CurrentRoom?.GetRoomItemHandler().GetItem(itemId);
                if (item == null || user.X != item.GetX || user.Y != item.GetY)
                    return;

                TriggerInstantTeleport(user, item);
            } catch (Exception e) {
                Plus.Core.ExceptionLogger.LogException(e);
            }
        }

        public static void CancelDelayedTeleport(int userId)
        {
            PendingTeleports.TryRemove(userId, out _);
        }

        public static bool TryTakeArrivalData(int userId, int roomId, out int targetItemId, out int bodyRotation, out int headRotation)
        {
            targetItemId = 0;
            bodyRotation = 0;
            headRotation = 0;

            if (!PendingArrivals.TryGetValue(userId, out ArrowTeleportArrivalData arrivalData) || arrivalData.TargetRoomId != roomId)
                return false;

            PendingArrivals.TryRemove(userId, out _);

            targetItemId = arrivalData.TargetItemId;
            bodyRotation = arrivalData.Rotation;
            headRotation = arrivalData.HeadRotation;
            return true;
        }

        public static void ClearArrivalData(int userId)
        {
            PendingArrivals.TryRemove(userId, out _);
        }
    }
}