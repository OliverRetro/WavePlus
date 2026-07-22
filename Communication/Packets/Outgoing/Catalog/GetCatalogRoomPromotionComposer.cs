using System.Collections.Generic;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Outgoing.Catalog
{
    internal class GetCatalogRoomPromotionComposer : MessageComposer
    {
        public List<RoomData> UsersRooms { get; }

        public GetCatalogRoomPromotionComposer(List<RoomData> usersRooms)
            : base(ServerPacketHeader.PromotableRoomsMessageComposer)
        {
            UsersRooms = usersRooms;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteBoolean(true); //wat
            packet.WriteInteger(UsersRooms.Count); //Count of rooms?
            foreach (RoomData room in UsersRooms) {
                packet.WriteInteger(room.Id);
                packet.WriteString(room.Name);
                packet.WriteBoolean(true);
            }
        }
    }
}