using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Outgoing.Rooms.FloorPlan
{
    internal class FloorPlanFloorMapComposer : MessageComposer
    {
        public ICollection<Item> Items { get; }

        public FloorPlanFloorMapComposer(ICollection<Item> items)
            : base(ServerPacketHeader.FloorPlanFloorMapMessageComposer)
        {
            Items = items;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Items.Count); //TODO: Figure this out, it pushes the room coords, but it iterates them, x,y|x,y|x,y|and so on.
            foreach (Item item in Items.ToList()) {
                packet.WriteInteger(item.GetX);
                packet.WriteInteger(item.GetY);
            }
        }
    }
}