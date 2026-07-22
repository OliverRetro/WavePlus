using System.Collections.Generic;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.HabboHotel.Cache.Type;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Groups
{
    internal class LeaveGroupEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int groupId = packet.PopInt();
            int userId = packet.PopInt();

            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(groupId, out Group group))
                return;

            if (group.CreatorId == userId) {
                session.SendNotification("Oops, the group owner cannot leave the group.");
                return;
            }

            // Captured before the removal, because DeleteMember now clears the badge itself and
            // the check below would otherwise always read as false.
            bool senderLostBadge = userId == session.GetHabbo().Id
                && session.GetHabbo().GetStats().FavouriteGroupId == groupId;

            if (group.IsMember(userId))
                group.DeleteMember(userId);

            if (group.IsAdmin(userId)) {
                if (group.IsAdmin(userId))
                    group.TakeAdmin(userId);

                if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(group.RoomId, out Room room))
                    return;

                RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
                if (user != null) {
                    user.RemoveStatus("flatctrl 1");
                    user.UpdateNeeded = true;

                    user.GetClient()?.SendPacket(new YouAreControllerComposer(0));
                }
            }

            session.SendPacket(new GroupInfoComposer(group, session));
            if (senderLostBadge) {
                if (group.AdminOnlyDeco == 0) {
                    if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(group.RoomId, out Room room))
                        return;

                    RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
                    if (user != null) {
                        user.RemoveStatus("flatctrl 1");
                        user.UpdateNeeded = true;

                        if (user.GetClient() != null)
                            user.GetClient().SendPacket(new YouAreControllerComposer(0));
                    }
                }
            }

            if (group.CreatorId == session.GetHabbo().Id || group.IsAdmin(session.GetHabbo().Id)) {
                if (!group.IsMember(userId))
                    return;

                if (userId == group.CreatorId)
                    return;

                if (group.IsAdmin(userId) && group.CreatorId != session.GetHabbo().Id) {
                    session.SendNotification(
                        "Sorry, only group creators can remove other administrators from the group.");
                    return;
                }

                if (group.IsAdmin(userId))
                    group.TakeAdmin(userId);

                if (group.IsMember(userId))
                    group.DeleteMember(userId);

                List<UserCache> members = [];
                List<int> memberIds = group.GetAllMembers;
                foreach (int id in memberIds.ToList()) {
                    UserCache groupMember = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(id);
                    if (groupMember == null)
                        continue;

                    if (!members.Contains(groupMember))
                        members.Add(groupMember);
                }

                int finishIndex = 14 < members.Count ? 14 : members.Count;
                int membersCount = members.Count;

                session.SendPacket(new GroupMembersComposer(group, [.. members.Take(finishIndex)], membersCount, 1,
                    (group.CreatorId == session.GetHabbo().Id || group.IsAdmin(session.GetHabbo().Id)), 0, ""));
            }
        }
    }
}