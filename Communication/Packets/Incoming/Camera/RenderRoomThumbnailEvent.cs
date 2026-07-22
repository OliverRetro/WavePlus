using log4net;
using Plus.Communication.Packets.Outgoing.Camera;
using Plus.HabboHotel.Camera;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Camera
{
    public class RenderRoomThumbnailEvent : IPacketEvent
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RenderRoomThumbnailEvent));

        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
                return;

            CameraManager camera = PlusEnvironment.GetGame().GetCameraManager();

            if (!camera.IsEnabled)
                return;

            if (!session.GetHabbo().InRoom)
                return;

            Room room = session.GetHabbo().CurrentRoom;

            if (room.OwnerId != session.GetHabbo().Id && !session.GetHabbo().GetPermissions().HasRight("mod_tool")) {
                Log.Warn("[Camera] User " + session.GetHabbo().Id + " tried to overwrite the thumbnail of room " + room.Id + " without being its owner.");
                return;
            }

            if (!camera.TryConsumeRenderSlot(session.GetHabbo().Id)) {
                session.SendPacket(new ThumbnailStatusComposer(false, true));
                return;
            }

            int declaredLength = packet.PopInt();

            if (declaredLength <= 0 || declaredLength > camera.MaxPhotoBytes || declaredLength > packet.RemainingLength())
                return;

            byte[] raw = packet.PopBytes(declaredLength);
            if (raw == null)
                return;

            byte[] sanitized = PngSanitizer.Sanitize(raw, camera.MaxPhotoDimension);
            if (sanitized == null) {
                Log.Warn("[Camera] User " + session.GetHabbo().Id + " uploaded a malformed or malicious thumbnail payload; rejected.");
                return;
            }

            if (!CameraManager.SavePng(camera.ThumbnailPath, "thumbnail_" + room.Id + ".png", sanitized)) {
                session.SendPacket(new ThumbnailStatusComposer(false));
                return;
            }

            session.SendPacket(new ThumbnailStatusComposer(true));
        }
    }
}