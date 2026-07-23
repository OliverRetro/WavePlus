using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Stock;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorStockVendor : IFurniInteractor
    {
        public void OnPlace(GameClient session, Item item)
        {
        }

        public void OnRemove(GameClient session, Item item)
        {
            // Picking the vendor up empties its shelves; the row survives so a re-placed vendor keeps its prices, it just has nothing on it until someone restocks.
            if (item?.GetRoom() != null)
                PlusEnvironment.GetRoomStockManager().ClearFurniStock(item.GetRoom().Id, item.Id);
        }

        /// <summary>
        /// Double-clicking the vendor raises the same buy window <c>:offer</c> uses, but scoped to
        /// this furni's own stock — <see cref="RpOfferService.Open"/> keys the pool by the furni's
        /// `items.id`, so each vendor sells only what <c>:additemstock</c> put on it. There's no
        /// seller, so no commission is paid and the window is titled after the furni.
        /// </summary>
        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
            Habbo habbo = session?.GetHabbo();
            Room room = item?.GetRoom();
            if (habbo == null || room == null)
                return;

            RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
            if (user == null || !Gamemap.TilesTouching(item.GetX, item.GetY, user.X, user.Y)) {
                session.SendWhisper("You need to stand next to it to see what's for sale.", 1);
                return;
            }

            if (!RpOfferService.Open(session, room.Id, item.Id, null, VendorTitle(item)))
                session.SendWhisper("There's nothing for sale here.", 1);
        }

        public void OnWiredTrigger(Item item)
        {
        }

        /// <summary>The furni's own name, falling back to a generic label.</summary>
        private static string VendorTitle(Item item)
        {
            string name = item.GetBaseItem()?.PublicName;
            return string.IsNullOrWhiteSpace(name) ? "Vendor" : name;
        }
    }
}
