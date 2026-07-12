using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Users.Inventory.Bots;

namespace Plus.HabboHotel.Catalog.Utilities
{
    public static class BotUtility
    {
        public static Bot CreateBot(ItemData itemData, int ownerId)
        {
            if (!PlusEnvironment.GetGame().GetCatalog().TryGetBot(itemData.Id, out CatalogBot cataBot))
                return null;

            bot botEntity = new()
            {
                user_id = (uint)ownerId,
                name = cataBot.Name,
                motto = cataBot.Motto,
                look = cataBot.Figure,
                gender = cataBot.Gender,
                ai_type = cataBot.AIType
            };

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                db.bots.Add(botEntity);
                db.SaveChanges();
            }

            return new Bot((int)botEntity.id, (int)botEntity.user_id, botEntity.name, botEntity.motto, botEntity.look, botEntity.gender);
        }


        public static BotAIType GetAIFromString(string type)
        {
            switch (type)
            {
                case "pet":
                    return BotAIType.Pet;
                case "generic":
                    return BotAIType.Generic;
                case "bartender":
                    return BotAIType.Bartender;
                case "casino_bot":
                    return BotAIType.CasinoBot;
                default:
                    return BotAIType.Generic;
            }
        }
    }
}