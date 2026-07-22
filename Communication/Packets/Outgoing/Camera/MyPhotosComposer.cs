using System.Collections.Generic;
using Plus.HabboHotel.Camera;

namespace Plus.Communication.Packets.Outgoing.Camera
{
    internal class MyPhotosComposer(IReadOnlyList<GalleryPhoto> photos) : MessageComposer(ServerPacketHeader.MyPhotosMessageComposer)
    {
        public IReadOnlyList<GalleryPhoto> Photos { get; } = photos;

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Photos.Count);

            foreach (GalleryPhoto photo in Photos) {
                packet.WriteString(photo.Url);
                packet.WriteInteger(photo.Timestamp);
                packet.WriteInteger(photo.RoomId);
            }
        }
    }
}