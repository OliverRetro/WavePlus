using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing;

namespace Plus.Communication.Packets.Outgoing.Rooms.Session
{
    internal class RoomReadyComposer : MessageComposer
    {
        private readonly Room _room;

        public RoomReadyComposer(Room room)
            : base(ServerPacketHeader.RoomReadyMessageComposer)
        {
            _room = room;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteString(_room.ModelName); 
            packet.WriteInteger(_room.Id);

            SerializeMapData(packet, _room);

            packet.WriteBoolean(true);
            packet.WriteInteger(_room.GetGameMap().StaticModel.WallHeight);
            packet.WriteString(_room.GetGameMap().StaticModel.Heightmap);
        }

        private void SerializeMapData(ServerPacket packet, Room room)
        {
            var map = room.GetGameMap().StaticModel;

            packet.WriteInteger(map.MapSizeX);
            packet.WriteInteger(map.MapSizeX * map.MapSizeY);

            for (int y = 0; y < map.MapSizeY; y++)
            {
                for (int x = 0; x < map.MapSizeX; x++)
                {
                    packet.WriteShort((short)(room.GetGameMap().SqAbsoluteHeight(x, y) * 256));
                }
            }
        }
    }
}