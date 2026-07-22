using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Packets.Outgoing.Users;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms;
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
            if (groups.Count >= 500) {
                session.SendPacket(new BroadcastMessageAlertComposer("Oops, it appears that you've hit the group membership limit! You can only join upto 1,500 groups."));
                return;
            }

            if (session.GetHabbo().GetStats().FavouriteGroupId != 0 && (group.Kind == GroupKind.Gang || group.Kind == GroupKind.Cartel || group.Kind == GroupKind.Mafia)) {
                session.SendPacket(new BroadcastMessageAlertComposer("Sorry, you can only join one gang at a time."));
                return;
            }

            if (GroupManager.IsWorkableKind(group.Kind) && session.GetHabbo().CorporationId != 0) {
                session.SendPacket(new BroadcastMessageAlertComposer("Sorry, you can only work for one corporation at a time."));
                return;
            }

            group.AddMember(session.GetHabbo().Id);

            // Only mark them employed once they're an ACTUAL member. For request-based (Locked)
            // corporations, applying just files a request — CorporationId is set when they're
            // hired. This lets a user apply to a new job without needing to reload first.
            if (GroupManager.IsWorkableKind(group.Kind) && group.IsMember(session.GetHabbo().Id))
                session.GetHabbo().CorporationId = group.Id;

            if (group.Type == GroupType.Locked) {
                List<GameClient> groupAdmins = (from client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList() where client != null && client.GetHabbo() != null && @group.IsAdmin(client.GetHabbo().Id) select client).ToList();
                foreach (GameClient client in groupAdmins) {
                    client.SendPacket(new GroupMembershipRequestedComposer(group.Id, session.GetHabbo(), 3));
                }

                session.SendPacket(new GroupInfoComposer(group, session));
            } else {
                if (session.GetHabbo().GetStats().FavouriteGroupId == 0 && (group.Kind == GroupKind.Gang || group.Kind == GroupKind.Cartel || group.Kind == GroupKind.Mafia)) {
                    session.GetHabbo().GetStats().FavouriteGroupId = group.Id;
                    int favGroupId = session.GetHabbo().GetStats().FavouriteGroupId;
                    int statsUserId = session.GetHabbo().Id;
                    using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                        db.UserStats.Where(x => x.Id == statsUserId).ExecuteUpdate(s => s.SetProperty(x => x.Groupid, favGroupId));
                    }

                    if (session.GetHabbo().InRoom && session.GetHabbo().CurrentRoom != null) {
                        session.GetHabbo().CurrentRoom.SendPacket(new RefreshFavouriteGroupComposer(session.GetHabbo().Id));
                        session.GetHabbo().CurrentRoom.SendPacket(new HabboGroupBadgesComposer(group));

                        RoomUser user = session.GetHabbo().CurrentRoom.GetRoomUserManager()
                            .GetRoomUserByHabbo(session.GetHabbo().Id);
                        if (user != null)
                            session.GetHabbo().CurrentRoom.SendPacket(new UpdateFavouriteGroupComposer(group, user.VirtualId));
                    } else
                        session.SendPacket(new RefreshFavouriteGroupComposer(session.GetHabbo().Id));
                }

                session.SendPacket(new GroupFurniConfigComposer(PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(session.GetHabbo().Id)));
                session.SendPacket(new GroupInfoComposer(group, session));
            }
        }
    }
}