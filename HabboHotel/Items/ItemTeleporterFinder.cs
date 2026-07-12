using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Items
{
    public static class ItemTeleporterFinder
    {
        public static int GetLinkedTele(int teleId)
        {
            uint tid = (uint) teleId;
            using WavePlusContext db = PlusEnvironment.GetDbContext();
            return db.room_items_tele_links.Where(x => x.tele_one_id == tid).Select(x => (int) x.tele_two_id).FirstOrDefault();
        }

        public static int GetTeleRoomId(int teleId, Room pRoom)
        {
            if (pRoom.GetRoomItemHandler().GetItem(teleId) != null)
                return pRoom.RoomId;

            uint tid = (uint) teleId;
            using WavePlusContext db = PlusEnvironment.GetDbContext();
            return db.items.Where(x => x.id == tid).Select(x => (int) x.room_id).FirstOrDefault();
        }

        public static bool IsTeleLinked(int teleId, Room pRoom)
        {
            int linkId = GetLinkedTele(teleId);

            if (linkId == 0) {
                return false;
            }


            Item item = pRoom.GetRoomItemHandler().GetItem(linkId);
            if (item != null && (item.GetBaseItem().InteractionType == InteractionType.Teleport || item.GetBaseItem().InteractionType == InteractionType.Arrow))
                return true;

            int roomId = GetTeleRoomId(linkId, pRoom);

            if (roomId == 0) {
                return false;
            }

            return true;
        }
    }
}