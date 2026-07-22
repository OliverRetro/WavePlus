using log4net;
using Newtonsoft.Json;
using Plus.Communication.Packets.Outgoing.Camera;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.Camera;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Incoming.Camera
{
    public class PurchasePhotoEvent : IPacketEvent
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PurchasePhotoEvent));

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

            // The same photo may be bought any number of times, but not faster than the cooldown.
            if (!camera.TryConsumePurchaseSlot(session.GetHabbo().Id))
                return;

            int price = camera.PurchasePrice;

            if (session.GetHabbo().Credits < price) {
                session.SendNotification("You do not have enough credits to buy this photo.");
                return;
            }

            if (!PlusEnvironment.GetGame().GetItemManager().GetItem(camera.PhotoItemId, out ItemData itemData)) {
                Log.Error("[Camera] server_settings key 'camera.item.id' does not point at a valid items_base row; photo purchases are broken until it does.");
                session.SendNotification("Photo purchasing is not configured correctly. Please contact staff.");
                return;
            }

            if (itemData.InteractionType != InteractionType.CameraPicture) {
                Log.Error("[Camera] items_base " + itemData.Id + " must have interaction_type 'camera_picture' to be sold as a photo.");
                session.SendNotification("Photo purchasing is not configured correctly. Please contact staff.");
                return;
            }

            string url = camera.PhotoUrl + "/" + photo.Filename;

            int photoId;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var row = new CameraPhotoEntity
                {
                    UserId = session.GetHabbo().Id,
                    RoomId = photo.RoomId,
                    Timestamp = photo.Timestamp,
                    Url = url
                };
                db.CameraPhotos.Add(row);
                db.SaveChanges();
                photoId = row.Id;
            }

            string extraData = JsonConvert.SerializeObject(new
            {
                t = photo.Timestamp,
                u = photoId.ToString(),
                s = photo.RoomId,
                n = session.GetHabbo().Username,
                m = "",
                w = url,
                oi = session.GetHabbo().Id,
                o = session.GetHabbo().Username
            });

            Item item = ItemFactory.CreateSingleItemNullable(itemData, session.GetHabbo(), extraData, extraData);

            if (item == null) {
                session.SendNotification("Something went wrong while creating your photo item.");
                return;
            }

            if (!session.GetHabbo().GetInventoryComponent().TryAddItem(item)) {
                session.SendNotification("Something went wrong while delivering your photo item.");
                return;
            }

            session.GetHabbo().Credits -= price;
            session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));

            session.SendPacket(new CameraPurchaseOkComposer());
            session.SendPacket(new FurniListNotificationComposer(item.Id, 1));
        }
    }
}