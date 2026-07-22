using System.Linq;
using Plus.Communication.Packets.Outgoing.Rooms.Furni.YouTubeTelevisions;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items.Televisions;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni.YouTubeTelevisions
{
    internal class YouTubeVideoInformationEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int itemId = packet.PopInt();
            string videoId = packet.PopString();

            foreach (TelevisionItem tele in PlusEnvironment.GetGame().GetTelevisionManager().TelevisionList.ToList()) {
                if (tele.YouTubeId != videoId)
                    continue;

                session.SendPacket(new GetYouTubeVideoComposer(itemId, tele.YouTubeId));
            }
        }
    }
}