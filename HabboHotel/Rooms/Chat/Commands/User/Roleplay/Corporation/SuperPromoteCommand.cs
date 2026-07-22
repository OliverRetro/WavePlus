using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Corporation
{
    internal class SuperPromoteCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_superpromote";

        public string Parameters => "%username%";

        public string Description => "Force-promote a member of this group, ignoring rank.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room?.Group == null)
                return;

            if (@params.Length < 2) {
                session.SendWhisper("Please enter the username of the person you want to promote.", 1);
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

            if (targetHabbo.Id == group.CreatorId) {
                session.SendWhisper("The group owner is already at the top.", 1);
                return;
            }

            if (!group.TryPromoteMember(targetHabbo.Id, out GroupRole nextRole)) {
                session.SendWhisper("That user is already at the highest available role.", 1);
                return;
            }

            PlusEnvironment.GetGame().GetAchievementManager().QueueProgress(targetClient, "ACH_Promotion", 1);

            session.SendPacket(new GroupMemberUpdatedComposer(group, targetHabbo, 2));
            session.SendPacket(new GroupInfoComposer(group, targetClient));
            targetHabbo.GetClient()?.SendPacket(new GroupInfoComposer(group, targetHabbo.GetClient()));
            LiveFeedService.LiveFeed("<b>" + LiveFeedService.Name(session.GetHabbo().Username, "green") + "</b> promoted <b>" + LiveFeedService.Name(targetHabbo.Username, "lightblue") + "</b> to <b>" + nextRole.Name + "</b>");

            RoomUser actor = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (actor != null)
                room.SendPacket(new ShoutComposer(actor.VirtualId, "*promoted " + targetHabbo.Username + " to " + nextRole.Name + "*", 0, session.GetHabbo().CustomBubbleId, isRpAction: true));
        }
    }
}