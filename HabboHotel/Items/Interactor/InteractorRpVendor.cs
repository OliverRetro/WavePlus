using System.Linq;
using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Cooldowns;
using Plus.HabboHotel.Roleplay.RpItem;
using Plus.HabboHotel.Roleplay.RpItem.Item;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.PathFinding;
using Plus.HabboHotel.Users;
using Plus.Utilities;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorRpVendor : IFurniInteractor
    {
        public void OnPlace(GameClient session, Item item)
        {
            item.ExtraData = "0";
            item.UpdateNeeded = true;
            Release(item);
        }

        public void OnRemove(GameClient session, Item item)
        {
            item.ExtraData = "0";
            Release(item);
        }

        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
            Habbo habbo = session?.GetHabbo();
            Room room = item?.GetRoom();

            if (habbo == null || room == null || item.ExtraData == "1" || item.InteractingUser != 0)
                return;

            if (item.GetBaseItem().VendingIds.Count < 1)
                return;

            RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
            if (user == null)
                return;

            if (habbo.GetRpStats()?.IsDead == true)
                return;

            if (!Gamemap.TilesTouching(user.X, user.Y, item.GetX, item.GetY)) {
                user.MoveTo(item.SquareInFront);
                return;
            }

            if (IsInventoryFull(habbo, item)) {
                session.SendWhisper("Your inventory is full (10 stacks max).", 1);
                return;
            }

            // Claimed here rather than on dispense so holding the button can't queue up machines.
            if (!PlusEnvironment.GetRpCooldownManager().TryConsume(session, RpCooldownKind.RpVendor,
                    "You must wait %seconds% second(s) before using a machine again."))
                return;

            item.InteractingUser = habbo.Id;

            user.CanWalk = false;
            user.ClearMovement(true);
            user.SetRot(Rotation.Calculate(user.X, user.Y, item.GetX, item.GetY), false);

            item.RequestUpdate(2, true);

            item.ExtraData = "1";
            item.UpdateState(false, true);
        }

        public void OnWiredTrigger(Item item)
        {
        }

        public static void Dispense(Item item)
        {
            Room room = item?.GetRoom();
            if (room == null)
                return;

            RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);
            user?.UnlockWalking();

            Habbo habbo = user?.GetClient()?.GetHabbo();
            if (habbo != null && item.GetBaseItem().VendingIds.Count > 0) {
                int itemId = item.GetBaseItem().VendingIds[RandomNumber.GenerateRandom(0, item.GetBaseItem().VendingIds.Count - 1)];
                Grant(habbo, itemId);
            }

            item.InteractingUser = 0;
            item.ExtraData = "0";
            item.UpdateState(false, true);
        }

        private static void Grant(Habbo habbo, int itemId)
        {
            UserRpItems items = habbo.GetRpItems();
            if (items == null)
                return;

            GameClient client = habbo.GetClient();

            UserRpItem granted = items.AddItem(itemId);
            if (granted == null) {
                client?.SendWhisper("The machine is out of order.", 1);
                return;
            }

            habbo.SaveRpItems();

            if (client != null) {
                WebOverlayCallbackEvent.RefreshInventory(client);
                client.SendWhisper($"You received {granted.ItemData?.Name ?? "an item"}.", 1);
            }
        }

        private static bool IsInventoryFull(Habbo habbo, Item item)
        {
            UserRpItems items = habbo.GetRpItems();
            if (items == null)
                return false;

            if (RpInventory.UsedSlots(habbo, 10) < 10)
                return false;

            // maybe inv full?
            foreach (int itemId in item.GetBaseItem().VendingIds) {
                if (!PlusEnvironment.GetRpItemManager().TryGetItem(itemId, out RpItemData def))
                    continue;

                if (def.IsStackable && items.GetItemsByItemId(itemId).Count() % def.StackLimit != 0)
                    return false;
            }

            return true;
        }

        private static void Release(Item item)
        {
            if (item.InteractingUser <= 0 || item.GetRoom() == null)
                return;

            RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(item.InteractingUser);
            if (user != null)
                user.CanWalk = true;

            item.InteractingUser = 0;
        }
    }
}