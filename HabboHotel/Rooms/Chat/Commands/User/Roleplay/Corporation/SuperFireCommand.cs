using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Corporation
{
    internal class SuperFireCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_superfire";

        public string Parameters => "%username%";

        public string Description => "Force-remove a member from this group, ignoring rank.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room?.Group == null)
                return;

            if (@params.Length < 2) {
                session.SendWhisper("Please enter the username of the person you want to fire.", 1);
                return;
            }

            Group group = room.Group;

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

            // Still off limits: the group would be left without an owner.
            if (targetHabbo.Id == group.CreatorId) {
                session.SendWhisper("You cannot fire the group owner.", 1);
                return;
            }

            // DeleteMember clears their employment and ends any shift they were on, so they're free
            // to be hired elsewhere right away without a reload.
            group.DeleteMember(targetHabbo.Id);

            session.SendPacket(new GroupMemberUpdatedComposer(group, targetHabbo, 2));
            session.SendPacket(new GroupInfoComposer(group, targetClient));
            targetHabbo.GetClient()?.SendPacket(new GroupInfoComposer(group, targetHabbo.GetClient()));
            LiveFeedService.LiveFeed(LiveFeedService.Name(session.GetHabbo().Username, "green") + " fired " + LiveFeedService.Name(targetHabbo.Username, "lightblue") + " from <b>" + group.Name + "</b>");

            RoomUser actor = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (actor != null)
                room.SendPacket(new ShoutComposer(actor.VirtualId, "*fired " + targetHabbo.Username + " from " + group.Name + "*", 0, session.GetHabbo().CustomBubbleId, isRpAction: true));

            targetHabbo.GetClient()?.SendWhisper("You've been removed from your role in '" + group.Name + "'.", 1);
        }
    }
}