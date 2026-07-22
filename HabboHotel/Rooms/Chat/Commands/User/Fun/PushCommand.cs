using System;
using System.Drawing;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    internal class PushCommand : IChatCommand
    {
        public string PermissionRequired => "command_push";

        public string Parameters => "%target%";

        public string Description => "Push another user.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1) {
                session.SendWhisper("Please enter the username of the user you wish to push.");
                return;
            }

            if (!room.PushEnabled && !session.GetHabbo().GetPermissions().HasRight("room_override_custom_config")) {
                session.SendWhisper("Oops, it appears that the room owner has disabled the ability to use the push command in here.");
                return;
            }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);
            if (targetClient == null) {
                session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
                return;
            }

            RoomUser targetUser = room.GetRoomUserManager().GetRoomUserByHabbo(targetClient.GetHabbo().Id);
            if (targetUser == null) {
                session.SendWhisper("An error occoured whilst finding that user, maybe they're not online or in this room.");
                return;
            }

            if (targetClient.GetHabbo().Username == session.GetHabbo().Username) {
                session.SendWhisper("Come on, surely you don't want to push yourself!");
                return;
            }

            if (targetUser.IsAsleep) {
                session.SendWhisper("Let's not bully the sleeping.", 1);
            }

            RoomUser thisUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (thisUser == null)
                return;

            if (!((Math.Abs(targetUser.X - thisUser.X) >= 2) || (Math.Abs(targetUser.Y - thisUser.Y) >= 2))) {
                int[] stepX = [0, 1, 1, 1, 0, -1, -1, -1];
                int[] stepY = [-1, -1, 0, 1, 1, 1, 0, -1];
                int rot = thisUser.RotBody & 7;
                int dx = stepX[rot];
                int dy = stepY[rot];

                Point tile = new(targetUser.X + dx, targetUser.Y + dy);
                if (room.GetGameMap().GetCoordinatedItems(tile).Any(i => i.GetBaseItem()?.InteractionType == InteractionType.Arrow)) {
                    session.SendWhisper("Please don't push that user onto an Arrow :(!");
                    return;
                }

                targetUser.MoveTo(tile.X, tile.Y);

                room.SendPacket(new ShoutComposer(thisUser.VirtualId, "*pushes " + @params[1] + " away from them*", 0, thisUser.LastBubble, isRpAction: true));
            } else {
                session.SendWhisper("Oops, " + @params[1] + " is not close enough!");
            }
        }
    }
}