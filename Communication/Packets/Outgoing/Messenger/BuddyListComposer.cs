using System;
using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Messenger;
using Plus.HabboHotel.Users.Relationships;

namespace Plus.Communication.Packets.Outgoing.Messenger
{
    internal class BuddyListComposer : MessageComposer
    {
        public ICollection<MessengerBuddy> Friends { get; }
        public Habbo Habbo { get; }
        public int Pages { get; }
        public int Page { get; }

        public BuddyListComposer(ICollection<MessengerBuddy> friends, Habbo player, int pages, int page)
            : base(ServerPacketHeader.BuddyListMessageComposer)
        {
            Friends = friends;
            Pages = pages;
            Habbo = player;
            Page = page;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Pages); // Pages
            packet.WriteInteger(Page); // Page

            packet.WriteInteger(Friends.Count);
            foreach (MessengerBuddy friend in Friends.ToList()) {
                Relationship relationship = Habbo.Relationships.FirstOrDefault(x => x.Value.UserId == Convert.ToInt32(friend.UserId)).Value;

                packet.WriteInteger(friend.Id);
                packet.WriteString(friend.MUsername);
                packet.WriteInteger(1); //Gender.
                packet.WriteBoolean(friend.IsOnline);
                packet.WriteBoolean(friend.IsOnline && friend.InRoom);
                packet.WriteString(friend.IsOnline ? friend.MLook : string.Empty);
                packet.WriteInteger(0); // category id
                packet.WriteString(friend.IsOnline ? friend.MMotto : string.Empty);
                packet.WriteString(string.Empty); //Alternative name?
                packet.WriteString(string.Empty);
                packet.WriteBoolean(true);
                packet.WriteBoolean(false);
                packet.WriteBoolean(false); //Pocket Habbo user.
                packet.WriteShort(relationship == null ? 0 : relationship.Type);
            }
        }
    }
}