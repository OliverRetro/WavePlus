using System.Collections.Generic;
using Plus.HabboHotel.Users.Badges;

namespace Plus.Communication.Packets.Outgoing.Inventory.Badges
{
    internal class BadgesComposer : MessageComposer
    {
        public ICollection<Badge> Badges { get; }

        public BadgesComposer(ICollection<Badge> badges)
            : base(ServerPacketHeader.BadgesMessageComposer)
        {
            Badges = badges;
        }

        public override void Compose(ServerPacket packet)
        {
            List<Badge> equippedBadges = new();

            packet.WriteInteger(Badges.Count);
            foreach (Badge badge in Badges) {
                packet.WriteInteger(1);
                packet.WriteString(badge.Code);

                if (badge.Slot > 0)
                    equippedBadges.Add(badge);
            }

            packet.WriteInteger(equippedBadges.Count);
            foreach (Badge badge in equippedBadges) {
                packet.WriteInteger(badge.Slot);
                packet.WriteString(badge.Code);
            }
        }
    }
}