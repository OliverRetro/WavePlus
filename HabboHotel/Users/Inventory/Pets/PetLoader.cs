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

            var petRows = db.Bots.Where(b => b.UserId == uid && b.RoomId == 0 && b.AiType == "pet")
                .Select(b => new { b.Id, b.UserId, b.RoomId, b.Name, b.X, b.Y, b.Z }).ToList();

            foreach (var row in petRows) {
                var mRow = db.BotsPetdata.Where(p => p.Id == row.Id)
                    .Select(p => new { p.Type, p.Race, p.Color, p.Experience, p.Energy, p.Nutrition, p.Respect, p.Createstamp, p.HaveSaddle, p.AnyoneRide, p.Hairdye, p.Pethair, p.GnomeClothing })
                    .FirstOrDefault();

                if (mRow != null) {
                    pets.Add(new Pet((int)row.Id, (int)row.UserId, (int)row.RoomId, row.Name, (int)(mRow.Type ?? 0), mRow.Race, mRow.Color,
                        mRow.Experience ?? 0, mRow.Energy ?? 0, mRow.Nutrition ?? 0, mRow.Respect ?? 0, (double)(mRow.Createstamp ?? 0), row.X, row.Y,
                        (double)row.Z, mRow.HaveSaddle ?? 0, mRow.AnyoneRide ?? 0, mRow.Hairdye ?? 0, mRow.Pethair ?? 0, mRow.GnomeClothing));
                }
            }

            return pets;
        }
    }
}