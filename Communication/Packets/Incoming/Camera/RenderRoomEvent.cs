using log4net;
using Plus.Communication.Packets.Outgoing.Camera;
using Plus.HabboHotel.Camera;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Camera
{
    public class RenderRoomEvent : IPacketEvent
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RenderRoomEvent));

        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
                return;

            CameraManager camera = PlusEnvironment.GetGame().GetCameraManager();

            if (!camera.IsEnabled) {
                session.SendNotification("The camera is currently disabled.");
                return;
            }

            if (!session.GetHabbo().InRoom)
                return;

            if (!camera.TryConsumeRenderSlot(session.GetHabbo().Id))
                return; // rate limited; drop silently

            int declaredLength = packet.PopInt();

            // Reject before buffering anything when the declared size is a lie or too big.
            if (declaredLength <= 0 || declaredLength > camera.MaxPhotoBytes || declaredLength > packet.RemainingLength()) {
                Log.Warn("[Camera] User " + session.GetHabbo().Id + " sent an invalid photo length (" + declaredLength + ").");
                return;
            }

            byte[] raw = packet.PopBytes(declaredLength);
            if (raw == null)
                return;

            byte[] sanitized = PngSanitizer.Sanitize(raw, camera.MaxPhotoDimension, out int width, out int height);
            if (sanitized == null) {
                Log.Warn("[Camera] User " + session.GetHabbo().Id + " uploaded a malformed or malicious photo payload; rejected.");
                return;
            }

            // Security check
            if (width != camera.PhotoSize || height != camera.PhotoSize) {
                Log.Warn("[Camera] User " + session.GetHabbo().Id + " uploaded a " + width + "x" + height + " photo (expected " + camera.PhotoSize + "x" + camera.PhotoSize + "); rejected.");
                return;
            }

            int timestamp = (int)PlusEnvironment.GetUnixTimestamp();
            string basename = session.GetHabbo().Id + "_" + timestamp + "_" + CameraManager.GenerateToken();
            string filename = basename + ".png";

            // The renderer's external-image visualization always loads "<name>_small.png"
            if (!CameraManager.SavePng(camera.PhotoPath, filename, sanitized) ||
                !CameraManager.SavePng(camera.PhotoPath, basename + "_small.png", sanitized)) {
                session.SendNotification("Your photo could not be saved. Please try again later.");
                return;
            }

            camera.SetPendingPhoto(new PendingPhoto(session.GetHabbo().Id, session.GetHabbo().CurrentRoomId, timestamp, filename));

            session.SendPacket(new CameraStorageUrlComposer(filename));
        }
    }
}