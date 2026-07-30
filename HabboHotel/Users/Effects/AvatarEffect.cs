using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects;
using Plus.Database.EF;
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

        public bool Activate()
        {
            double tsNow = UnixTimestamp.GetNow();
            int id = Id;

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                db.UserEffects.Where(e => e.Id == id).ExecuteUpdate(s => s.SetProperty(e => e.IsActivated, "1").SetProperty(e => e.ActivatedStamp, tsNow));

            Activated = true;
            TimestampActivated = tsNow;
            return true;
        }

        public void HandleExpiration(Habbo habbo)
        {
            Quantity--;

            Activated = false;
            TimestampActivated = 0;

            int id = Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                if (Quantity < 1) {
                    db.UserEffects.Where(e => e.Id == id).ExecuteDelete();
                } else {
                    int qt = Quantity;
                    db.UserEffects.Where(e => e.Id == id).ExecuteUpdate(s => s
                        .SetProperty(e => e.Quantity, qt)
                        .SetProperty(e => e.IsActivated, "0")
                        .SetProperty(e => e.ActivatedStamp, (double?)0));
                }
            }

            habbo.GetClient().SendPacket(new AvatarEffectExpiredComposer(this));
            // reset fx if in room?
        }

        public void AddToQuantity()
        {
            Quantity++;

            int id = Id;
            int qt = Quantity;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                db.UserEffects.Where(e => e.Id == id).ExecuteUpdate(s => s.SetProperty(e => e.Quantity, qt));
        }
    }
}