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
            var rows = (from it in db.items
                        where it.room_id == rid
                        join g in db.items_groups on it.id equals g.id into gj
                        from g in gj.DefaultIfEmpty()
                        select new
                        {
                            it.id, it.room_id, it.base_item, it.extra_data, it.x, it.y, it.z, it.rot, it.user_id,
                            group_id = (int?)g.group_id,
                            it.limited_number, it.limited_stack, it.wall_pos
                        }).ToList();

            foreach (var row in rows)
            {
                if (PlusEnvironment.GetGame().GetItemManager().GetItem((int)row.base_item, out ItemData Data)) {
                    items.Add(new Item((int)row.id, (int)row.room_id, (int)row.base_item, row.extra_data,
                        row.x, row.y, row.z, row.rot, row.user_id,
                        row.group_id ?? 0, row.limited_number ?? 0, row.limited_stack ?? 0, row.wall_pos, room));
                }
            }

            return items;
        }

        public static List<Item> GetItemsForUser(int userId)
        {
            List<Item> I = new();

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            var rows = (from it in db.items
                        where it.room_id == 0 && it.user_id == userId
                        join g in db.items_groups on it.id equals g.id into gj
                        from g in gj.DefaultIfEmpty()
                        select new
                        {
                            it.id, it.room_id, it.base_item, it.extra_data, it.x, it.y, it.z, it.rot, it.user_id,
                            group_id = (int?)g.group_id,
                            it.limited_number, it.limited_stack, it.wall_pos
                        }).ToList();

            foreach (var row in rows)
            {
                if (PlusEnvironment.GetGame().GetItemManager().GetItem((int)row.base_item, out ItemData data)) {
                    I.Add(new Item((int)row.id, (int)row.room_id, (int)row.base_item, row.extra_data,
                        row.x, row.y, row.z, row.rot, row.user_id,
                        row.group_id ?? 0, row.limited_number ?? 0, row.limited_stack ?? 0, row.wall_pos));
                }
            }

            return I;
        }
    }
}
