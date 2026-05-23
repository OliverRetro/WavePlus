using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Outgoing.Groups
{
    internal class GroupInfoComposer : MessageComposer
    {
        public Group Group { get; }
        public bool NewWindow { get; }
        public Habbo Habbo { get; }
        public DateTime Origin { get; }

        public GroupInfoComposer(Group group, GameClient session, bool newWindow = false)
            : base(ServerPacketHeader.GroupInfoMessageComposer)
        {
            Origin = DateTimeOffset
    .FromUnixTimeSeconds(group.CreateTime)
    .DateTime;
            Group = group;
            NewWindow = newWindow;
            Habbo = session.GetHabbo();
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Group.Id);
            packet.WriteBoolean(true);
            packet.WriteInteger(Group.Type == GroupType.Open ? 0 : Group.Type == GroupType.Locked ? 1 : 2);
            packet.WriteString(Group.Name);
            packet.WriteString(Group.Description);
            packet.WriteString(Group.Badge);
            packet.WriteInteger(Group.RoomId);
            packet.WriteString(Group.GetRoom() != null ? Group.GetRoom().Name : "No room found.."); // room name
            packet.WriteInteger(Group.CreatorId == Habbo.Id ? 3 : Group.HasRequest(Habbo.Id) ? 2 : Group.IsMember(Habbo.Id) ? 1 : 0);
            packet.WriteInteger(Group.MemberCount); // Members
            packet.WriteBoolean(false); //?? CHANGED
            packet.WriteString(Origin.ToString("dd-MM-yyyy"));
            packet.WriteBoolean(Group.CreatorId == Habbo.Id);
            packet.WriteBoolean(Group.IsAdmin(Habbo.Id)); // admin
            packet.WriteString(PlusEnvironment.GetUsernameById(Group.CreatorId));
            packet.WriteBoolean(NewWindow); // Show group info
            packet.WriteBoolean(Group.AdminOnlyDeco == 0); // Any user can place furni in home room
            packet.WriteInteger(Group.CreatorId == Habbo.Id ? Group.RequestCount : Group.IsAdmin(Habbo.Id) ? Group.RequestCount : Group.IsMember(Habbo.Id) ? 0 : 0); // Pending users
            //base.WriteInteger(0);//what the fuck
            packet.WriteBoolean(Group != null ? Group.ForumEnabled : true); //HabboTalk.
        }
    }
}