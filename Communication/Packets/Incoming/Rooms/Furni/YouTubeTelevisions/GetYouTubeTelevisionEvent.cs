using System;
using System.Collections.Generic;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Rooms.Furni.YouTubeTelevisions;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items.Televisions;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni.YouTubeTelevisions
{
    internal class GetYouTubeTelevisionEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
                return;

            int itemId = packet.PopInt();
            ICollection<TelevisionItem> videos = PlusEnvironment.GetGame().GetTelevisionManager().TelevisionList;
            if (videos.Count == 0)
            {
                session.SendNotification("Oh, it looks like the hotel manager haven't added any videos for you to watch! :(");
                return;
            }

            Dictionary<int, TelevisionItem> dict = PlusEnvironment.GetGame().GetTelevisionManager().Televisions;
            foreach (TelevisionItem value in RandomValues(dict).Take(1))
            {
                session.SendPacket(new GetYouTubeVideoComposer(itemId, value.YouTubeId));
            }

            session.SendPacket(new GetYouTubePlaylistComposer(itemId, videos));
        }

        private static IEnumerable<TValue> RandomValues<TKey, TValue>(IDictionary<TKey, TValue> dict)
        {
            Random rand = new();
            List<TValue> values = dict.Values.ToList();
            int size = dict.Count;
            while (true)
            {
                yield return values[rand.Next(size)];
            }
        }
    }
}