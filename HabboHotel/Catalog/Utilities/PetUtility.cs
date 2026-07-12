using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.Rooms.AI;

namespace Plus.HabboHotel.Catalog.Utilities
{
    public static class PetUtility
    {
        public static bool CheckPetName(string name)
        {
            if (name.Length < 1 || name.Length > 16)
                return false;

            return PlusEnvironment.IsValidAlphaNumeric(name);
        }

        public static Pet CreatePet(int userId, string name, int type, string race, string colour)
        {
            Pet pet = new(0, userId, 0, name, type, race, colour, 0, 100, 100, 0, PlusEnvironment.GetUnixTimestamp(), 0, 0, 0.0, 0, 0, 0, -1, "-1");

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                // motto/look are NOT NULL without a scaffolded model default; set empty to match the legacy partial insert.
                bot botEntity = new()
                {
                    user_id = (uint)pet.OwnerId,
                    name = pet.Name,
                    ai_type = "pet",
                    motto = "",
                    look = ""
                };
                db.bots.Add(botEntity);
                db.SaveChanges();
                pet.PetId = (int)botEntity.id;

                // Explicit PK (mirrors bots.id) → raw insert; UNIX_TIMESTAMP() kept as literal SQL.
                db.Database.ExecuteSqlInterpolated($"INSERT INTO bots_petdata (id,type,race,color,experience,energy,createstamp) VALUES ({pet.PetId}, {pet.Type}, {pet.Race}, {pet.Color}, 0, 100, UNIX_TIMESTAMP())");
            }

            return pet;
        }
    }
}