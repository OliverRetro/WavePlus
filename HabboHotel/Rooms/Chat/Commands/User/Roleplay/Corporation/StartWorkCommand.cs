using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Users;
using System.Linq;
using System.Text;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Corporation
{
    internal class StartWorkCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_start_work";

        public string Parameters => "";

        public string Description => "Start your work shift in the corporation.";
        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room?.Group == null || room == null)
                return;

            Habbo habbo = session.GetHabbo();
            Group group = room.Group;

            if(group.Kind != GroupKind.Corporation)
            {
                session.SendWhisper("This command can only be used in corporation room!", 1);
                return;
            }

            if (!group.IsMember(habbo.Id))
            {
                session.SendWhisper("You don't work here! Feel free to apply to join the corporation.", 1);
                return;
            }

            if (group.TryGetRoleData(habbo.Id, out GroupRole role))
            {
                session.SendWhisper("Your role is: " + role.Name + ". Motto: " + role.ShiftMotto + ". Look " + role.ShiftCostume + ". Shift length: " + role.ShiftDuration + " minutes.", 1);

                // Motto handling. We cache the old motto, and then we set the new one. When the user finishes their shift, we can revert it back to the old one.
                if (role.ShiftMotto != string.Empty || role.ShiftMotto != null) {
                    habbo.OldMotto = habbo.Motto;
                    habbo.Motto = role.ShiftMotto;
                }

                if (role.ShiftCostume != string.Empty || role.ShiftCostume != null)
                {
                    habbo.OldLook = habbo.Look;
                    habbo.Look = role.ShiftCostume;
                }

                // update the user, regardless if they have a costume or motto change
                RoomUser user = room?.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);

                room.SendPacket(new UserChangeComposer(user, false));
                return;
            } else {
                session.SendWhisper("You don't have a role in this corporation! Contact your superior to get assigned a role.", 1);
                return;
            }
        }
    }
}
