using System;
using System.Data;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Items
{
    public static class ItemTeleporterFinder
    {
        public static int GetLinkedTele(int teleId)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `tele_two_id` FROM `room_items_tele_links` WHERE `tele_one_id` = '" + teleId + "' LIMIT 1");
                DataRow row = dbClient.GetRow();

                if (row == null) {
                    return 0;
                }

                return Convert.ToInt32(row[0]);
            }
        }

        public static int GetTeleRoomId(int teleId, Room pRoom)
        {
            if (pRoom.GetRoomItemHandler().GetItem(teleId) != null)
                return pRoom.RoomId;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `room_id` FROM `items` WHERE `id` = " + teleId + " LIMIT 1");
                DataRow row = dbClient.GetRow();

                if (row == null) {
                    return 0;
                }

                return Convert.ToInt32(row[0]);
            }
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