using System.Linq;
using Plus.HabboHotel.Cache.Type;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Outgoing.Rooms.Settings
{
    internal class RoomRightsListComposer : MessageComposer
    {
        public Room Room { get; }

        public RoomRightsListComposer(Room instance)
            : base(ServerPacketHeader.RoomRightsListMessageComposer)
        {
            Room = instance;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Room.Id);

            packet.WriteInteger(Room.UsersWithRights.Count);
            foreach (int id in Room.UsersWithRights.ToList())
            {
                UserCache data = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(id);
                if (data == null)
                {
                    packet.WriteInteger(0);
                    packet.WriteString("Unknown Error");
                }
                else
                {
                    packet.WriteInteger(data.Id);
                    packet.WriteString(data.Username);
                }
            }
        }
    }
}