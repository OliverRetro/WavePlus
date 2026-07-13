using System.Collections.Generic;
using Plus.HabboHotel.Cache.Type;

namespace Plus.Communication.Packets.Outgoing.Rooms.Settings
{
    internal class GetRoomBannedUsersComposer : MessageComposer
    {
        public int RoomId { get; }
        public List<int> BannedUsers { get; }

        public GetRoomBannedUsersComposer(int roomId, List<int> bannedUsers)
            : base(ServerPacketHeader.GetRoomBannedUsersMessageComposer)
        {
            RoomId = roomId;
            BannedUsers = bannedUsers;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(RoomId);

            packet.WriteInteger(BannedUsers.Count); //Count
            foreach (int id in BannedUsers)
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