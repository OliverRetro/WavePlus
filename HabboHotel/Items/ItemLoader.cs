using System.Collections.Generic;
using System.Linq;
using Plus.Database.EF;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Items
{
    public static class ItemLoader
    {
        public static List<Item> GetItemsForRoom(int roomId, Room room)
        {
            List<Item> items = new();
            uint rid = (uint)roomId;

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            var rows = (from it in db.Items
                        where it.RoomId == rid
                        join g in db.ItemsGroups on it.Id equals g.Id into gj
                        from g in gj.DefaultIfEmpty()
                        select new
                        {
                            it.Id,
                            it.RoomId,
                            it.BaseItem,
                            it.ExtraData,
                            it.X,
                            it.Y,
                            it.Z,
                            it.Rot,
                            it.UserId,
                            group_id = (int?)g.GroupId,
                            it.LimitedNumber,
                            it.LimitedStack,
                            it.WallPos
                        }).ToList();

            foreach (var row in rows) {
                if (PlusEnvironment.GetGame().GetItemManager().GetItem((int)row.BaseItem, out ItemData Data)) {
                    items.Add(new Item((int)row.Id, (int)row.RoomId, (int)row.BaseItem, row.ExtraData,
                        row.X, row.Y, row.Z, row.Rot, row.UserId,
                        row.group_id ?? 0, row.LimitedNumber ?? 0, row.LimitedStack ?? 0, row.WallPos, room));
                }
            }

            return items;
        }

        public static List<Item> GetItemsForUser(int userId)
        {
            List<Item> I = new();

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            var rows = (from it in db.Items
                        where it.RoomId == 0 && it.UserId == userId
                        join g in db.ItemsGroups on it.Id equals g.Id into gj
                        from g in gj.DefaultIfEmpty()
                        select new
                        {
                            it.Id,
                            it.RoomId,
                            it.BaseItem,
                            it.ExtraData,
                            it.X,
                            it.Y,
                            it.Z,
                            it.Rot,
                            it.UserId,
                            group_id = (int?)g.GroupId,
                            it.LimitedNumber,
                            it.LimitedStack,
                            it.WallPos
                        }).ToList();

            foreach (var row in rows) {
                if (PlusEnvironment.GetGame().GetItemManager().GetItem((int)row.BaseItem, out ItemData data)) {
                    I.Add(new Item((int)row.Id, (int)row.RoomId, (int)row.BaseItem, row.ExtraData,
                        row.X, row.Y, row.Z, row.Rot, row.UserId,
                        row.group_id ?? 0, row.LimitedNumber ?? 0, row.LimitedStack ?? 0, row.WallPos));
                }
            }

            return I;
        }
    }
}