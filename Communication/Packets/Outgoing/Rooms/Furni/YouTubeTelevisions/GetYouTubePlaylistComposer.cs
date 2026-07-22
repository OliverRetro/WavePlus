using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.Items.Televisions;

namespace Plus.Communication.Packets.Outgoing.Rooms.Furni.YouTubeTelevisions
{
    internal class GetYouTubePlaylistComposer : MessageComposer
    {
        public int ItemId { get; }
        public ICollection<TelevisionItem> Videos { get; }

        public GetYouTubePlaylistComposer(int itemId, ICollection<TelevisionItem> videos)
            : base(ServerPacketHeader.GetYouTubePlaylistMessageComposer)
        {
            ItemId = itemId;
            Videos = videos;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(ItemId);
            packet.WriteInteger(Videos.Count);
            foreach (TelevisionItem video in Videos.ToList()) {
                packet.WriteString(video.YouTubeId);
                packet.WriteString(video.Title); //Title
                packet.WriteString(video.Description); //Description
            }

            packet.WriteString("");
        }
    }
}