using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Groups
{
    internal class AcceptGroupMembershipEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int groupId = packet.PopInt();
            int userId = packet.PopInt();

            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(groupId, out Group group))
                return;

            if (!group.IsOwnerOrHasPermission(session.GetHabbo().Id, "hire_user") && !session.GetHabbo().GetPermissions().HasRight("fuse_group_accept_any"))
                return;

            if (!group.HasRequest(userId))
                return;

            Habbo habbo = PlusEnvironment.GetHabboById(userId);
            if (habbo == null) {
                session.SendNotification("Oops, an error occurred whilst finding this user.");
                return;
            }

            // check for gang configuration, if they are trying to accept a gang request, but gangs are disabled, don't let them.
            if (habbo.GetStats().FavouriteGroupId == 0 && (group.Kind == GroupKind.Gang || group.Kind == GroupKind.Cartel || group.Kind == GroupKind.Mafia)) {
                session.GetHabbo().GetStats().FavouriteGroupId = group.Id;
                int favGroupId = session.GetHabbo().GetStats().FavouriteGroupId;
                int statsUserId = session.GetHabbo().Id;
                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    db.UserStats.Where(x => x.Id == statsUserId).ExecuteUpdate(s => s.SetProperty(x => x.Groupid, favGroupId));
                }

                if (habbo.InRoom && habbo.CurrentRoom != null) {
                    habbo.CurrentRoom.SendPacket(new RefreshFavouriteGroupComposer(habbo.Id));
                    habbo.CurrentRoom.SendPacket(new HabboGroupBadgesComposer(group));

                    RoomUser user = habbo.CurrentRoom.GetRoomUserManager()
                        .GetRoomUserByHabbo(habbo.Id);
                    if (user != null)
                        habbo.CurrentRoom.SendPacket(new UpdateFavouriteGroupComposer(group, user.VirtualId));
                }
                else
                    session.SendPacket(new RefreshFavouriteGroupComposer(habbo.Id));
            }

            group.HandleRequest(userId, true);

            session.SendPacket(new GroupMemberUpdatedComposer(group, habbo, 4));
        }
    }
}