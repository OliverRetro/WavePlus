using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Corporation
{
    internal class HireCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_hire_anygroup";
        public string GroupPermissionRequired => "hire_user";

        public string Parameters => "%username%";

        public string Description => "Invite a user to join this group.";

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

            if (group.IsMember(targetHabbo.Id)) {
                session.SendWhisper("That user is already a member of this group.", 1);
                return;
            }

            if (GroupManager.IsWorkableKind(group.Kind) && targetHabbo.CorporationId != 0) {
                session.SendWhisper("That user already works for another corporation.", 1);
                return;
            }

            // If they already applied there's nothing to ask — just accept their request.
            if (group.HasRequest(targetHabbo.Id)) {
                GroupInviteService.Hire(session, group, targetClient, targetHabbo);
                Shout(session, room, "*hired " + targetHabbo.Username + " to " + group.Name + "*");
                return;
            }

            // Otherwise raise the invite popup in their overlay — they have to be online.
            if (targetClient == null) {
                session.SendWhisper("That user is offline, so they can't be invited right now.", 1);
                return;
            }

            if (!GroupInviteService.Invite(session, group, targetClient))
                return;

            session.SendWhisper("Invited " + targetHabbo.Username + " to " + group.Name + ". Waiting for them to accept.", 1);
            Shout(session, room, GroupInviteService.InviteShout(group, targetHabbo.Username));
        }

        private static void Shout(GameClient session, Room room, string message)
        {
            RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user != null)
                room.SendPacket(new ShoutComposer(user.VirtualId, message, 0, session.GetHabbo().CustomBubbleId, isRpAction: true));
        }
    }
}