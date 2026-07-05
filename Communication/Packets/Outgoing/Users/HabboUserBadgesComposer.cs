using System.Linq;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Badges;

namespace Plus.Communication.Packets.Outgoing.Users
{
    internal class HabboUserBadgesComposer : MessageComposer
    {
        public Habbo Habbo { get; }

        public HabboUserBadgesComposer(Habbo habbo)
            : base(ServerPacketHeader.HabboUserBadgesMessageComposer)
        {
            Habbo = habbo;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Habbo.Id);
            packet.WriteInteger(Habbo.GetBadgeComponent().EquippedCount);

            foreach (Badge badge in Habbo.GetBadgeComponent().GetBadges().Where(b => b.Slot > 0).ToList())
            {
                packet.WriteInteger(badge.Slot);
                packet.WriteString(badge.Code);
            }
        }
    }
}