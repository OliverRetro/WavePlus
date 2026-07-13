using System;
using System.Collections.Generic;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class MassDanceCommand : IChatCommand
    {
        public string PermissionRequired => "command_massdance";

        public string Parameters => "%DanceId%";

        public string Description => "Force everyone in the room to dance to a dance of your choice.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1)
            {
                session.SendWhisper("Please enter a dance ID. (1-4)");
                return;
            }

            int danceId = Convert.ToInt32(@params[1]);
            if (danceId < 0 || danceId > 4)
            {
                session.SendWhisper("Please enter a dance ID. (1-4)");
                return;
            }

            List<RoomUser> users = room.GetRoomUserManager().GetRoomUsers();
            if (users.Count > 0)
            {
                foreach (RoomUser u in users.ToList())
                {
                    if (u == null)
                        continue;

                    if (u.CarryItemId > 0)
                        u.CarryItemId = 0;

                    u.DanceId = danceId;
                    room.SendPacket(new DanceComposer(u.VirtualId, danceId));
                }
            }
        }
    }
}