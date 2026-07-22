using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;

namespace Plus.HabboHotel.Items
{
    public static class ItemHopperFinder
    {
        public static int GetAHopper(int curRoom)
        {
            // NOTE: no EF entity exists for `items_hopper`; scalar read via raw SQL through the context.
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                return db.Database
                    .SqlQuery<int>($"SELECT room_id AS Value FROM items_hopper WHERE room_id <> {curRoom} ORDER BY room_id ASC LIMIT 1")
                    .AsEnumerable()
                    .FirstOrDefault();
            }
        }

        public static int GetHopperId(int nextRoom)
        {
            // NOTE: no EF entity exists for `items_hopper`; scalar read via raw SQL through the context.
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                return db.Database
                    .SqlQuery<int>($"SELECT hopper_id AS Value FROM items_hopper WHERE room_id = {nextRoom} LIMIT 1")
                    .AsEnumerable()
                    .FirstOrDefault();
            }
        }
    }
}