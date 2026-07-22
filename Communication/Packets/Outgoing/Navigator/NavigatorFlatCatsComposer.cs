using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.Navigator;

namespace Plus.Communication.Packets.Outgoing.Navigator
{
    internal class NavigatorFlatCatsComposer : MessageComposer
    {
        public ICollection<SearchResultList> Categories { get; }

        public NavigatorFlatCatsComposer(ICollection<SearchResultList> categories)
            : base(ServerPacketHeader.NavigatorFlatCatsMessageComposer)
        {
            Categories = categories;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Categories.Count);
            foreach (SearchResultList category in Categories.ToList()) {
                packet.WriteInteger(category.Id);
                packet.WriteString(category.PublicName);
                packet.WriteBoolean(true); // TODO
            }
        }
    }
}