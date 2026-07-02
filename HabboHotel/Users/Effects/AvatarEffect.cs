using Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects;
using Plus.Database.Interfaces;
using Plus.Utilities;

namespace Plus.HabboHotel.Users.Effects
{
    public sealed class AvatarEffect
    {
        public AvatarEffect(int id, int userId, int spriteId, double duration, bool activated, double timestampActivated, int quantity)
        {
            Id = id;
            UserId = userId;
            SpriteId = spriteId;
            Duration = duration;
            Activated = activated;
            TimestampActivated = timestampActivated;
            Quantity = quantity;
        }

        public int Id { get; set; }

        public int UserId { get; set; }

        public int SpriteId { get; set; }

        public double Duration { get; set; }

        public bool Activated { get; set; }

        public double TimestampActivated { get; set; }

        public int Quantity { get; set; }

        public double TimeUsed => (UnixTimestamp.GetNow() - TimestampActivated);

        public double TimeLeft
        {
            get
            {
                double tl = (Activated ? Duration - TimeUsed : Duration);

                if (tl < 0) {
                    tl = 0;
                }

                return tl;
            }
        }

        public bool HasExpired => (Activated && TimeLeft <= 0);

        /// <summary>
        /// Activates the AvatarEffect
        /// </summary>
        public bool Activate()
        {
            double tsNow = UnixTimestamp.GetNow();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `user_effects` SET `is_activated` = '1', `activated_stamp` = @ts WHERE `id` = @id");
                dbClient.AddParameter("ts", tsNow);
                dbClient.AddParameter("id", Id);
                dbClient.RunQuery();

                Activated = true;
                TimestampActivated = tsNow;
                return true;
            }
        }

        public void HandleExpiration(Habbo habbo)
        {
            Quantity--;

            Activated = false;
            TimestampActivated = 0;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                if (Quantity < 1) {
                    dbClient.SetQuery("DELETE FROM `user_effects` WHERE `id` = @id");
                    dbClient.AddParameter("id", Id);
                    dbClient.RunQuery();
                } else {
                    dbClient.SetQuery("UPDATE `user_effects` SET `quantity` = @qt, `is_activated` = '0', `activated_stamp` = 0 WHERE `id` = @id");
                    dbClient.AddParameter("qt", Quantity);
                    dbClient.AddParameter("id", Id);
                    dbClient.RunQuery();
                }
            }

            habbo.GetClient().SendPacket(new AvatarEffectExpiredComposer(this));
            // reset fx if in room?
        }

        public void AddToQuantity()
        {
            Quantity++;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `user_effects` SET `quantity` = @qt WHERE `id` = @id");
                dbClient.AddParameter("qt", Quantity);
                dbClient.AddParameter("id", Id);
                dbClient.RunQuery();
            }
        }
    }
}