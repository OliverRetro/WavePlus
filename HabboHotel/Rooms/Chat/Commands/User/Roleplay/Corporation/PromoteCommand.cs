using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Corporation
{
    internal class PromoteCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_promote_anygroup";
        public string GroupPermissionRequired => "promote_user";

        public string Parameters => "%username%";

        public string Description => "Promote a group member.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room?.Group == null)
                return;

            if (@params.Length < 2)
            {
                session.SendWhisper("Please enter the username of the person you want to promote.", 1);
                return;
            }

            Group group = room.Group;
            if (!session.GetHabbo().GetPermissions().HasCommand(PermissionRequired) && !group.IsOwnerOrHasPermission(session.GetHabbo().Id, GroupPermissionRequired))
            {
                session.SendWhisper("Oops, you do not have permission to promote users for this group.", 1);
                return;
            }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);
            Habbo targetHabbo = targetClient?.GetHabbo() ?? PlusEnvironment.GetHabboByUsername(@params[1]);
            if (targetHabbo == null)
            {
                session.SendWhisper("Oops, couldn't find that user.", 1);
                return;
            }

            if (!group.IsMember(targetHabbo.Id))
            {
                session.SendWhisper("That user is not a member of this group.", 1);
                return;
            }

            if (targetHabbo.Id == group.CreatorId)
            {
                session.SendWhisper("You cannot promote the group owner.", 1);
                return;
            }

            if (!group.TryPromoteMember(targetHabbo.Id, out GroupRole nextRole))
            {
                session.SendWhisper("That user is already at the highest available role.", 1);
                return;
            }

            session.SendPacket(new GroupMemberUpdatedComposer(group, targetHabbo, 2));
            session.SendPacket(new GroupInfoComposer(group, targetClient));
            targetHabbo.GetClient()?.SendPacket(new GroupInfoComposer(group, targetHabbo.GetClient()));
            session.SendWhisper("You promoted " + targetHabbo.Username + " to " + nextRole.Name + ".", 1);
            targetHabbo.GetClient()?.SendWhisper("You were promoted to " + nextRole.Name + " in '" + group.Name + "'.", 1);
        }
    }
}
