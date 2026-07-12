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
            return db.bots
                .Where(b => b.user_id == uid && b.room_id == 0 && b.ai_type != "pet")
                .Select(b => new { b.id, b.user_id, b.name, b.motto, b.look, b.gender })
                .AsEnumerable()
                .Select(b => new Bot((int)b.id, (int)b.user_id, b.name, b.motto, b.look, b.gender))
                .ToList();
        }
    }
}
