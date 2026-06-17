using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Users;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Corporation
{
    internal class StopWorkCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_stop_work";

        public string Parameters => "";

        public string Description => "Stop your work shift in the corporation.";
        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room?.Group == null || room == null)
                return;

            Habbo habbo = session.GetHabbo();
            Group group = room.Group;

            if (group.Kind != GroupKind.Corporation)
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
                habbo.Motto = habbo.OldMotto;
                habbo.Look = habbo.OldLook;

                // update the user, regardless if they have a costume or motto change
                RoomUser user = room?.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);

                room.SendPacket(new UserChangeComposer(user, false));
                return;
            }
            else
            {
                session.SendWhisper("You don't have a role in this corporation! Contact your superior to get assigned a role.", 1);
                return;
            }
        }
    }
}
