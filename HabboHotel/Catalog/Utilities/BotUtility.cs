using Plus.Database.EF;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms.AI;

namespace Plus.HabboHotel.Catalog.Utilities
{
    public static class BotUtility
    {
        public static Users.Inventory.Bots.Bot CreateBot(ItemData itemData, int ownerId)
        {
            if (!PlusEnvironment.GetGame().GetCatalog().TryGetBot(itemData.Id, out CatalogBot cataBot))
                return null;

            Database.EF.Entities.BotEntity botEntity = new()
            {
                UserId = (uint)ownerId,
                Name = cataBot.Name,
                Motto = cataBot.Motto,
                Look = cataBot.Figure,
                Gender = cataBot.Gender,
                AiType = cataBot.AIType
            };

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.Bots.Add(botEntity);
                db.SaveChanges();
            }

            return new Users.Inventory.Bots.Bot((int)botEntity.Id, (int)botEntity.UserId, botEntity.Name, botEntity.Motto, botEntity.Look, botEntity.Gender);
        }

        public static BotAIType GetAIFromString(string type)
        {
            switch (type) {
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