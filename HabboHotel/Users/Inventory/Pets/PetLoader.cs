using System.Collections.Generic;
using System.Linq;
using Plus.Database.EF;
using Plus.HabboHotel.Rooms.AI;

namespace Plus.HabboHotel.Users.Inventory.Pets
{
    internal static class PetLoader
    {
        public static List<Pet> GetPetsForUser(int userId)
        {
            List<Pet> pets = new();

            uint uid = (uint)userId;
            using WavePlusContext db = PlusEnvironment.GetDbContext();

            var petRows = db.bots.Where(b => b.user_id == uid && b.room_id == 0 && b.ai_type == "pet")
                .Select(b => new { b.id, b.user_id, b.room_id, b.name, b.x, b.y, b.z }).ToList();

            foreach (var row in petRows)
            {
                var mRow = db.bots_petdata.Where(p => p.id == row.id)
                    .Select(p => new { p.type, p.race, p.color, p.experience, p.energy, p.nutrition, p.respect, p.createstamp, p.have_saddle, p.anyone_ride, p.hairdye, p.pethair, p.gnome_clothing })
                    .FirstOrDefault();

                if (mRow != null) {
                    pets.Add(new Pet((int)row.id, (int)row.user_id, (int)row.room_id, row.name, (int)(mRow.type ?? 0), mRow.race, mRow.color,
                        mRow.experience ?? 0, mRow.energy ?? 0, mRow.nutrition ?? 0, mRow.respect ?? 0, (double)(mRow.createstamp ?? 0), row.x, row.y,
                        (double)row.z, mRow.have_saddle ?? 0, mRow.anyone_ride ?? 0, mRow.hairdye ?? 0, mRow.pethair ?? 0, mRow.gnome_clothing));
                }
            }

            return pets;
        }
    }
}
