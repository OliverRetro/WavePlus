using Plus.Database.EF;
using Plus.Database.EF.Entities;

namespace Plus.HabboHotel.Users.Effects
{
    internal static class AvatarEffectFactory
    {
        /// <summary>
        /// Creates a new AvatarEffect with the specified details.
        /// </summary>
        /// <param name="habbo"></param>
        /// <param name="spriteId"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static AvatarEffect CreateNullable(Habbo habbo, int spriteId, double duration)
        {
            using WavePlusContext db = PlusEnvironment.GetDbContext();
            user_effect row = new()
            {
                user_id = (uint)habbo.Id,
                effect_id = spriteId,
                total_duration = (int)duration,
                is_activated = "0",
                activated_stamp = 0,
                quantity = 1
            };
            db.user_effects.Add(row);
            db.SaveChanges();

            return new AvatarEffect(row.id, habbo.Id, spriteId, duration, false, 0, 1);
        }
    }
}
