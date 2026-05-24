using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Users;

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

            if (@params.Length < 2)
            {
                session.SendWhisper("Please enter the username of the person you want to hire.", 1);
                return;
            }

            Group group = room.Group;
            if (!session.GetHabbo().GetPermissions().HasCommand(PermissionRequired) && !group.IsOwnerOrHasPermission(session.GetHabbo().Id, GroupPermissionRequired))
            {
                session.SendWhisper("Oops, you do not have permission to hire users for this group.", 1);
                return;
            }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);
            Habbo targetHabbo = targetClient?.GetHabbo() ?? PlusEnvironment.GetHabboByUsername(@params[1]);
            if (targetHabbo == null)
            {
                session.SendWhisper("Oops, couldn't find that user.", 1);
                return;
            }

            if (!group.HasRequest(targetHabbo.Id))
            {
                session.SendWhisper("That user does not have a pending request for this group.", 1);
                return;
            }

            group.HandleRequest(targetHabbo.Id, true);

            session.SendPacket(new GroupMemberUpdatedComposer(group, targetHabbo, 4));
            session.SendPacket(new GroupInfoComposer(group, targetClient));
            session.SendWhisper("You hired " + targetHabbo.Username + ".", 1);
            targetHabbo.GetClient()?.SendWhisper("Your request to join '" + group.Name + "' was accepted.", 1);
        }
    }
}
