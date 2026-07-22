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

        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
            Habbo habbo = session?.GetHabbo();
            if (habbo == null || item?.GetRoom() == null)
                return;

            RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
            if (user == null || !Gamemap.TilesTouching(item.GetX, item.GetY, user.X, user.Y))
                return;

            if (!RpOfferService.Open(session, item.GetRoom().Id, item.Id, null, "Vendor"))
                session.SendWhisper("This vendor has nothing in stock.", 1);
        }

        public void OnWiredTrigger(Item item)
        {
        }
    }
}
