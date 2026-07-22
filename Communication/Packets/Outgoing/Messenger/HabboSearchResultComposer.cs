using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.Users.Messenger;

namespace Plus.Communication.Packets.Outgoing.Messenger
{
    internal class HabboSearchResultComposer : MessageComposer
    {
        public List<SearchResult> Friends { get; }
        public List<SearchResult> OtherUsers { get; }

        public HabboSearchResultComposer(List<SearchResult> friends, List<SearchResult> otherUsers)
            : base(ServerPacketHeader.HabboSearchResultMessageComposer)
        {
            Friends = friends;
            OtherUsers = otherUsers;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Friends.Count);
            foreach (SearchResult friend in Friends.ToList()) {
                bool online = (PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(friend.UserId) != null);

                packet.WriteInteger(friend.UserId);
                packet.WriteString(friend.Username);
                packet.WriteString(friend.Motto);
                packet.WriteBoolean(online);
                packet.WriteBoolean(false);
                packet.WriteString(string.Empty);
                packet.WriteInteger(0);
                packet.WriteString(online ? friend.Figure : "");
                packet.WriteString(friend.LastOnline);
            }

            packet.WriteInteger(OtherUsers.Count);
            foreach (SearchResult otherUser in OtherUsers.ToList()) {
                bool online = (PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(otherUser.UserId) != null);

                packet.WriteInteger(otherUser.UserId);
                packet.WriteString(otherUser.Username);
                packet.WriteString(otherUser.Motto);
                packet.WriteBoolean(online);
                packet.WriteBoolean(false);
                packet.WriteString(string.Empty);
                packet.WriteInteger(0);
                packet.WriteString(online ? otherUser.Figure : "");
                packet.WriteString(otherUser.LastOnline);
            }
        }
    }
}