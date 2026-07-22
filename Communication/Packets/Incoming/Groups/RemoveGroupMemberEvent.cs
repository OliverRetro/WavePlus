using System.Collections.Generic;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.Cache.Type;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Groups
{
    internal class RemoveGroupMemberEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int groupId = packet.PopInt();
            int userId = packet.PopInt();

            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(groupId, out Group group))
                return;

            if (userId == session.GetHabbo().Id) {
                if (group.CreatorId == userId) {
                    session.SendNotification("Oops, the group owner cannot leave the group.");
                    return;
                }

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

                        if (user.GetClient() != null)
                            user.GetClient().SendPacket(new YouAreControllerComposer(0));
                    }
                }

                session.SendPacket(new GroupInfoComposer(group, session));
                if (session.GetHabbo().GetStats().FavouriteGroupId == groupId) {
                    session.GetHabbo().GetStats().FavouriteGroupId = 0;
                    using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                        db.UserStats.Where(x => x.Id == userId).ExecuteUpdate(s => s.SetProperty(x => x.Groupid, 0));
                    }

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

                    if (session.GetHabbo().InRoom && session.GetHabbo().CurrentRoom != null) {
                        RoomUser user = session.GetHabbo().CurrentRoom.GetRoomUserManager()
                            .GetRoomUserByHabbo(session.GetHabbo().Id);
                        if (user != null)
                            session.GetHabbo().CurrentRoom
                                .SendPacket(new UpdateFavouriteGroupComposer(group, user.VirtualId));
                        session.GetHabbo().CurrentRoom
                            .SendPacket(new RefreshFavouriteGroupComposer(session.GetHabbo().Id));
                    } else
                        session.SendPacket(new RefreshFavouriteGroupComposer(session.GetHabbo().Id));
                }

                return;
            }

            if (group.CreatorId == session.GetHabbo().Id || group.IsAdmin(session.GetHabbo().Id)) {
                if (!group.IsMember(userId))
                    return;

                if (userId == group.CreatorId)
                    return;

                if (group.IsAdmin(userId) && group.CreatorId != session.GetHabbo().Id) {
                    session.SendNotification("Sorry, only group creators can remove other administrators from the group.");
                    return;
                }

                if (group.IsAdmin(userId))
                    group.TakeAdmin(userId);

                if (group.IsMember(userId))
                    group.DeleteMember(userId);

                List<UserCache> members = new();
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

                session.SendPacket(new GroupMembersComposer(group, members.Take(finishIndex).ToList(), membersCount, 1,
                    (group.CreatorId == session.GetHabbo().Id || group.IsAdmin(session.GetHabbo().Id)), 0, ""));
            }
        }
    }
}