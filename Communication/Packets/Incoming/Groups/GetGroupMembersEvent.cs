using System.Collections.Generic;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.Cache.Type;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Incoming.Groups
{
    internal class GetGroupMembersEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int groupId = packet.PopInt();
            int page = packet.PopInt();
            string searchVal = packet.PopString();
            int requestType = packet.PopInt();

            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(groupId, out Group group))
                return;

            List<int> ids = requestType switch
            {
                1 => group.GetAdministrators,
                2 => group.GetRequests,
                _ => group.GetAllMembers
            };

            List<UserCache> members = [.. ids.Select(id => PlusEnvironment.GetGame().GetCacheManager().GenerateUser(id)).Where(user => user != null).Distinct()];

            if (!string.IsNullOrWhiteSpace(searchVal))
            {
                members = members.Where(x => x.Username.StartsWith(searchVal)).ToList();
            }

            int startIndex = (page - 1) * 14;
            List<UserCache> pagedMembers = members.Skip(startIndex).Take(14).ToList();

            session.SendPacket(new GroupMembersComposer(group, pagedMembers, members.Count, page, (group.CreatorId == session.GetHabbo().Id || group.IsAdmin(session.GetHabbo().Id)), requestType, searchVal));
        }
    }
}