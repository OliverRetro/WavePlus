using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Users;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Users;
using System.Linq;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Corporation
{
    internal class HireCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_hire_anygroup";
        public string GroupPermissionRequired => "hire_user";

        public string Parameters => "%username%";

        public string Description => "Accept a pending group member request.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room?.Group == null)
                return;

            if (@params.Length < 2) {
                session.SendWhisper("Please enter the username of the person you want to hire.", 1);
                return;
            }

            Group group = room.Group;
            if (!session.GetHabbo().GetPermissions().HasCommand(PermissionRequired) && !group.IsOwnerOrHasPermission(session.GetHabbo().Id, GroupPermissionRequired)) {
                session.SendWhisper("Oops, you do not have permission to hire users for this group.", 1);
                return;
            }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);
            Habbo targetHabbo = targetClient?.GetHabbo() ?? PlusEnvironment.GetHabboByUsername(@params[1]);
            if (targetHabbo == null) {
                session.SendWhisper("Oops, couldn't find that user.", 1);
                return;
            }

            if (!group.HasRequest(targetHabbo.Id)) {
                session.SendWhisper("That user does not have a pending request for this group.", 1);
                return;
            }

            if (GroupManager.IsWorkableKind(group.Kind) && targetHabbo.CorporationId != 0) {
                session.SendWhisper("That user already works for another corporation.", 1);
                return;
            }

            if (targetHabbo.GetStats().FavouriteGroupId == 0 && (group.Kind == GroupKind.Gang || group.Kind == GroupKind.Cartel || group.Kind == GroupKind.Mafia)) {
                targetHabbo.GetStats().FavouriteGroupId = group.Id;
                int gid = targetHabbo.GetStats().FavouriteGroupId;
                int uid = targetHabbo.Id;
                using WavePlusContext db = PlusEnvironment.GetDbContext();
                db.user_stats.Where(x => x.id == uid).ExecuteUpdate(s => s.SetProperty(x => x.groupid, gid));

                if (targetHabbo.InRoom && targetHabbo.CurrentRoom != null) {
                    targetHabbo.CurrentRoom.SendPacket(new RefreshFavouriteGroupComposer(targetHabbo.Id));
                    targetHabbo.CurrentRoom.SendPacket(new HabboGroupBadgesComposer(group));

                    RoomUser user = targetHabbo.CurrentRoom.GetRoomUserManager()
                        .GetRoomUserByHabbo(targetHabbo.Id);
                    if (user != null)
                        targetHabbo.CurrentRoom.SendPacket(new UpdateFavouriteGroupComposer(group, user.VirtualId));
                } else
                    session.SendPacket(new RefreshFavouriteGroupComposer(targetHabbo.Id));
            }

            group.HandleRequest(targetHabbo.Id, true);

            if (GroupManager.IsWorkableKind(group.Kind))
                targetHabbo.CorporationId = group.Id;

            session.SendPacket(new GroupMemberUpdatedComposer(group, targetHabbo, 4));
            session.SendPacket(new GroupInfoComposer(group, targetClient));
            LiveFeedService.LiveFeed(LiveFeedService.Name(session.GetHabbo().Username, "green") + " hired " + LiveFeedService.Name(targetHabbo.Username, "lightblue") + " to <b>" + group.Name + "</b>");
            room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*" + session.GetHabbo().Username + " hired " + targetHabbo.Username + " to " + group.Name + "*", 0, session.GetHabbo().CustomBubbleId));
        }
    }
}
