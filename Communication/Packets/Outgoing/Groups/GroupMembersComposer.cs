using System.Collections.Generic;
using Plus.HabboHotel.Cache.Type;
using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Outgoing.Groups
{
    internal class GroupMembersComposer : MessageComposer
    {
        public Group Group { get; }
        public ICollection<UserCache> Members { get; }
        public int MembersCount { get; }
        public int Page { get; }
        public bool Admin { get; }
        public int ReqType { get; }
        public string SearchVal { get; }

        public GroupMembersComposer(Group group, ICollection<UserCache> members, int membersCount, int page, bool admin, int reqType, string searchVal)
            : base(ServerPacketHeader.GroupMembersMessageComposer)
        {
            Group = group;
            Members = members;
            MembersCount = membersCount;
            Page = page;
            Admin = admin;
            ReqType = reqType;
            SearchVal = searchVal;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Group.Id);
            packet.WriteString(Group.Name);
            packet.WriteInteger(Group.RoomId);
            packet.WriteString(Group.Badge);
            packet.WriteInteger(MembersCount);

            packet.WriteInteger(Members.Count);
            if (MembersCount > 0)
            {
                foreach (UserCache data in Members)
                {
                    packet.WriteInteger(Group.CreatorId == data.Id ? 0 : Group.IsAdmin(data.Id) ? 1 : Group.IsMember(data.Id) ? 2 : 3);
                    packet.WriteInteger(data.Id);
                    packet.WriteString(data.Username);
                    packet.WriteString(data.Look);
                    packet.WriteString(Group.GetMemberRoleName(data.Id));
                }
            }

            packet.WriteBoolean(Admin);
            packet.WriteInteger(14);
            packet.WriteInteger(Page);
            packet.WriteInteger(ReqType);
            packet.WriteString(SearchVal);
        }
    }
}
