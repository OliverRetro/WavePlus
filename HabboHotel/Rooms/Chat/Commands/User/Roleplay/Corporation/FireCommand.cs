using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Corporation
{
    internal class FireCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_fire_anygroup";
        public string GroupPermissionRequired => "kick_from_group";

        public string Parameters => "%username%";

        public string Description => "Remove a group member.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room?.Group == null)
                return;

            if (@params.Length < 2) {
                session.SendWhisper("Please enter the username of the person you want to fire.", 1);
                return;
            }

            Group group = room.Group;
            if (!session.GetHabbo().GetPermissions().HasCommand(PermissionRequired) && !group.IsOwnerOrHasPermission(session.GetHabbo().Id, GroupPermissionRequired)) {
                session.SendWhisper("Oops, you do not have permission to fire users for this group.", 1);
                return;
            }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);
            Habbo targetHabbo = targetClient?.GetHabbo() ?? PlusEnvironment.GetHabboByUsername(@params[1]);
            if (targetHabbo == null) {
                session.SendWhisper("Oops, couldn't find that user.", 1);
                return;
            }

            if (!group.IsMember(targetHabbo.Id)) {
                session.SendWhisper("That user is not a member of this group.", 1);
                return;
            }

            if (targetHabbo.Id == group.CreatorId) {
                session.SendWhisper("You cannot fire the group owner.", 1);
                return;
            }

            if (!GroupHierarchy.CanActOn(session, group, targetHabbo.Id)) {
                session.SendWhisper("You cannot fire someone at a higher group level than your own.", 1);
                return;
            }

            // DeleteMember clears their employment and ends any shift they were on, so they're free
            // to be hired elsewhere right away without a reload.
            if (group.IsMember(targetHabbo.Id))
                group.DeleteMember(targetHabbo.Id);

            session.SendPacket(new GroupMemberUpdatedComposer(group, targetHabbo, 2));
            session.SendPacket(new GroupInfoComposer(group, targetClient));
            targetHabbo.GetClient()?.SendPacket(new GroupInfoComposer(group, targetHabbo.GetClient()));
            LiveFeedService.LiveFeed(LiveFeedService.Name(session.GetHabbo().Username, "green") + " fired " + LiveFeedService.Name(targetHabbo.Username, "lightblue") + " from <b>" + group.Name + "</b>");
            room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*fired " + targetHabbo.Username + " from " + group.Name + "*", 0, session.GetHabbo().CustomBubbleId, isRpAction: true));
            targetHabbo.GetClient()?.SendWhisper("You've been removed from your role in '" + group.Name + "'.", 1);
        }
    }
}