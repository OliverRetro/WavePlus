using Plus.Communication.Packets.Outgoing.Camera;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.Camera;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Camera
{
    public class PublishPhotoEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
                return;

            CameraManager camera = PlusEnvironment.GetGame().GetCameraManager();

            if (!camera.IsEnabled)
                return;

            PendingPhoto photo = camera.GetPendingPhoto(session.GetHabbo().Id);

            if (photo == null)
                return;

            // Each photo can be published to the website exactly once.
            if (photo.Published) {
                session.SendPacket(new CameraPublishStatusComposer(false, 0));
                return;
            }

            int wait = camera.GetPublishWait(session.GetHabbo().Id);

            if (wait > 0) {
                session.SendPacket(new CameraPublishStatusComposer(false, wait));
                return;
            }

            int price = camera.PublishPrice;

            if (session.GetHabbo().Credits < price) {
                session.SendNotification("You do not have enough credits to publish this photo.");
                session.SendPacket(new CameraPublishStatusComposer(false, 0));
                return;
            }

            string url = camera.PhotoUrl + "/" + photo.Filename;

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.CameraWeb.Add(new CameraWebEntity
                {
                    UserId = session.GetHabbo().Id,
                    RoomId = photo.RoomId,
                    Timestamp = (int)PlusEnvironment.GetUnixTimestamp(),
                    Url = url
                });
                db.SaveChanges();
            }

            photo.Published = true;
            camera.StampPublish(session.GetHabbo().Id);

            session.GetHabbo().Credits -= price;
            session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));

            session.SendPacket(new CameraPublishStatusComposer(true, camera.PublishCooldownSeconds, url));
        }
    }
}