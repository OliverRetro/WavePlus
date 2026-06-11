using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.Database.Interfaces;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using System.Collections.Generic;
using System.Linq;

namespace Plus.Communication.Packets.Incoming.Groups
{
    internal class JoinGroupEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null)
                return;

            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(packet.PopInt(), out Group group))
                return;

            if (group.IsMember(session.GetHabbo().Id) || group.IsAdmin(session.GetHabbo().Id) || (group.HasRequest(session.GetHabbo().Id) && group.Type == GroupType.Private))
                return;

            List<Group> groups = PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(session.GetHabbo().Id);
            if (groups.Count >= 500)
            {
                session.SendPacket(new BroadcastMessageAlertComposer("Oops, it appears that you've hit the group membership limit! You can only join upto 1,500 groups."));
                return;
            }

            if(session.GetHabbo().GetStats().FavouriteGroupId != 0 && (group.Kind == GroupKind.Gang || group.Kind == GroupKind.Cartel || group.Kind == GroupKind.Mafia))
            {
                session.SendPacket(new BroadcastMessageAlertComposer("Sorry, you can only join one gang at a time."));
                return;
            }

            group.AddMember(session.GetHabbo().Id);

            if (group.Type == GroupType.Locked)
            {
                List<GameClient> groupAdmins = (from client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList() where client != null && client.GetHabbo() != null && @group.IsAdmin(client.GetHabbo().Id) select client).ToList();
                foreach (GameClient client in groupAdmins)
                {
                    client.SendPacket(new GroupMembershipRequestedComposer(group.Id, session.GetHabbo(), 3));
                }

                session.SendPacket(new GroupInfoComposer(group, session));
            }
            else
            {
                if (session.GetHabbo().GetStats().FavouriteGroupId == 0 && (group.Kind == GroupKind.Gang || group.Kind == GroupKind.Cartel || group.Kind == GroupKind.Mafia))
                {
                    session.GetHabbo().GetStats().FavouriteGroupId = group.Id;
                    using IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor();
                    dbClient.SetQuery("UPDATE `user_stats` SET `groupid` = @groupId WHERE `id` = @userId LIMIT 1");
                    dbClient.AddParameter("groupId", session.GetHabbo().GetStats().FavouriteGroupId);
                    dbClient.AddParameter("userId", session.GetHabbo().Id);
                    dbClient.RunQuery();

                    if (session.GetHabbo().InRoom && session.GetHabbo().CurrentRoom != null)
                    {
                        session.GetHabbo().CurrentRoom.SendPacket(new RefreshFavouriteGroupComposer(session.GetHabbo().Id));
                        session.GetHabbo().CurrentRoom.SendPacket(new HabboGroupBadgesComposer(group));

                        RoomUser user = session.GetHabbo().CurrentRoom.GetRoomUserManager()
                            .GetRoomUserByHabbo(session.GetHabbo().Id);
                        if (user != null)
                            session.GetHabbo().CurrentRoom.SendPacket(new UpdateFavouriteGroupComposer(group, user.VirtualId));
                    }
                    else
                        session.SendPacket(new RefreshFavouriteGroupComposer(session.GetHabbo().Id));
                }

                session.SendPacket(new GroupFurniConfigComposer(PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(session.GetHabbo().Id)));
                session.SendPacket(new GroupInfoComposer(group, session));
            }
        }
    }
}