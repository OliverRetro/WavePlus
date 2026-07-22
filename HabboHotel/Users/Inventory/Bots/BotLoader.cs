using System.Collections.Generic;
using System.Linq;
using Plus.Database.EF;

namespace Plus.HabboHotel.Users.Inventory.Bots
{
    internal class BotLoader
    {
        public static List<Bot> GetBotsForUser(int userId)
        {
            uint uid = (uint)userId;

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            return db.Bots
                .Where(b => b.UserId == uid && b.RoomId == 0 && b.AiType != "pet")
                .Select(b => new { b.Id, b.UserId, b.Name, b.Motto, b.Look, b.Gender })
                .AsEnumerable()
                .Select(b => new Bot((int)b.Id, (int)b.UserId, b.Name, b.Motto, b.Look, b.Gender))
                .ToList();
        }
    }
}