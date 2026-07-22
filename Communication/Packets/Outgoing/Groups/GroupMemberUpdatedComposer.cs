using Plus.HabboHotel.Users;
using GroupEntity = Plus.HabboHotel.Groups.Group;

namespace Plus.Communication.Packets.Outgoing.Groups
{
    internal class GroupMemberUpdatedComposer : MessageComposer
    {
        public GroupEntity Group { get; }
        public int GroupId { get; }
        public Habbo Habbo { get; }
        public int Type { get; }

        public GroupMemberUpdatedComposer(GroupEntity group, Habbo habbo, int type)
            : base(ServerPacketHeader.GroupMemberUpdatedMessageComposer)
        {
            Group = group;
            GroupId = group.Id;
            Habbo = habbo;
            Type = type;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(GroupId); //GroupId
            packet.WriteInteger(Type); //Type?
            {
                packet.WriteInteger(Habbo.Id); //UserId
                packet.WriteString(Habbo.Username);
                packet.WriteString(Habbo.Look);
                packet.WriteString(Group.GetMemberRoleName(Habbo.Id));
            }
        }
    }
}