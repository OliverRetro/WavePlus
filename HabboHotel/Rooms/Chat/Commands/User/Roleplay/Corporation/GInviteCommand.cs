using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Corporation
{
    internal class GInviteCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_ginvite";

        public string Parameters => "%username%";

        public string Description => "Invite a user to your gang from anywhere.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null)
                return;

            if (@params.Length < 2) {
                session.SendWhisper("Please enter the username of the person you want to invite.", 1);
                return;
            }

            int groupId = session.GetHabbo().GetStats().FavouriteGroupId;
            if (groupId == 0 || !PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(groupId, out Group group) || group == null) {
                session.SendWhisper("You aren't wearing a group badge, so there's nothing to invite anyone to.", 1);
                return;
            }

            if (!group.IsOwnerOrHasPermission(session.GetHabbo().Id, "hire_user")) {
                session.SendWhisper("Oops, you do not have permission to invite users to " + group.Name + ".", 1);
                return;
            }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);
            Habbo targetHabbo = targetClient?.GetHabbo();
            if (targetHabbo == null) {
                session.SendWhisper("That user is offline, so they can't be invited right now.", 1);
                return;
            }

            if (targetHabbo.Id == session.GetHabbo().Id) {
                session.SendWhisper("You're already in " + group.Name + ".", 1);
                return;
            }

            if (group.IsMember(targetHabbo.Id)) {
                session.SendWhisper("That user is already a member of " + group.Name + ".", 1);
                return;
            }

            if (targetHabbo.GetStats().FavouriteGroupId != 0) {
                session.SendWhisper("User already has a gang.", 1);
                return;
            }

            // Already applied? Then there's nothing to ask — accept them straight away.
            if (group.HasRequest(targetHabbo.Id)) {
                GroupInviteService.Hire(session, group, targetClient, targetHabbo);
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