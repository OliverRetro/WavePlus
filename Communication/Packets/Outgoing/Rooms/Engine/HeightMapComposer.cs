using Plus.HabboHotel.Rooms;
using System;

namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class HeightMapComposer : MessageComposer
    {
        private readonly Room _room;

        public HeightMapComposer(Room room)
            : base(ServerPacketHeader.HeightMapMessageComposer)
        {
            _room = room;
        }

        public override void Compose(ServerPacket packet)
        {
            var map = _room.GetGameMap().StaticModel;

            packet.WriteInteger(map.MapSizeX);
            packet.WriteInteger(map.MapSizeX * map.MapSizeY);

            for (int y = 0; y < map.MapSizeY; y++)
            {
                for (int x = 0; x < map.MapSizeX; x++)
                {
                    packet.WriteShort((short)(_room.GetGameMap().SqAbsoluteHeight(x, y) * 256));
                }
            }
        }
    }
}