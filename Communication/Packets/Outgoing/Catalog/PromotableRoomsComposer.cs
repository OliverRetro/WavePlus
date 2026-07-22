using System.Collections.Generic;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Outgoing.Catalog
{
    internal class PromotableRoomsComposer : MessageComposer
    {
        public ICollection<RoomData> Rooms { get; }

        public PromotableRoomsComposer(ICollection<RoomData> rooms)
            : base(ServerPacketHeader.PromotableRoomsMessageComposer)
        {
            Rooms = rooms;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteBoolean(true);
            packet.WriteInteger(Rooms.Count); //Count

            foreach (RoomData data in Rooms) {
                packet.WriteInteger(data.Id);
                packet.WriteString(data.Name);
                packet.WriteBoolean(false);
            }
        }
    }
}