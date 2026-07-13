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
            UserEffect row = new()
            {
                UserId = (uint)habbo.Id,
                EffectId = spriteId,
                TotalDuration = (int)duration,
                IsActivated = "0",
                ActivatedStamp = 0,
                Quantity = 1
            };
            db.UserEffects.Add(row);
            db.SaveChanges();

            return new AvatarEffect(row.Id, habbo.Id, spriteId, duration, false, 0, 1);
        }
    }
}
