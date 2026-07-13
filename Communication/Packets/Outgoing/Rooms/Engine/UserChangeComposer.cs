using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.AI;

namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class UserChangeComposer : MessageComposer
    {
        public RoomUser User { get; }
        public bool Self { get; }

        public RoomBot BotData { get; }
        public int VirtualId { get; }

        public UserChangeComposer(RoomUser user, bool self)
            : base(ServerPacketHeader.UserChangeMessageComposer)
        {
            User = user;
            VirtualId = user.VirtualId;
            Self = self;
        }

        public UserChangeComposer(int virtualId, RoomBot botData)
            : base(ServerPacketHeader.UserChangeMessageComposer)
        {
            VirtualId = virtualId;
            BotData = botData;
        }

        public override void Compose(ServerPacket packet)
        {
            if (BotData == null)
            {
                packet.WriteInteger((Self) ? -1 : VirtualId);
                packet.WriteString(User.GetClient().GetHabbo().Look);
                packet.WriteString(User.GetClient().GetHabbo().Gender);
                packet.WriteString(User.GetClient().GetHabbo().Motto);
                packet.WriteInteger(User.GetClient().GetHabbo().GetStats().AchievementPoints);
            }
            else
            {
                packet.WriteInteger(VirtualId);
                packet.WriteString(BotData.Look);
                packet.WriteString(BotData.Gender);
                packet.WriteString(BotData.Motto);
                packet.WriteInteger(0);
            }
        }
    }
}