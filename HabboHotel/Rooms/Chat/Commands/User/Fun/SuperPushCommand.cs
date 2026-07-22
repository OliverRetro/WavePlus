using System;
using System.Drawing;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    internal class SuperPushCommand : IChatCommand
    {
        public string PermissionRequired => "command_super_push";

        public string Parameters => "%target%";

        public string Description => "Superpush another user. (Pushes them 3 squares away)";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1) {
                session.SendWhisper("Please enter the username of the user you wish to push.", 1);
                return;
            }

            if (!room.SuperPushEnabled && !room.CheckRights(session, true) && !session.GetHabbo().GetPermissions().HasRight("room_override_custom_config")) {
                session.SendWhisper("Oops, it appears that the room owner has disabled the ability to use the push command in here.", 1);
                return;
            }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);
            if (targetClient == null) {
                session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.", 1);
                return;
            }

            RoomUser targetUser = room.GetRoomUserManager().GetRoomUserByHabbo(targetClient.GetHabbo().Id);
            if (targetUser == null) {
                session.SendWhisper("An error occoured whilst finding that user, maybe they're not online or in this room.", 1);
                return;
            }

            if (targetUser.IsAsleep) {
                session.SendWhisper("Let's not bully the sleeping.", 1);
            }

            if (targetClient.GetHabbo().Username == session.GetHabbo().Username) {
                session.SendWhisper("Come on, surely you don't want to push yourself!", 1);
                return;
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

                Gamemap map = room.GetGameMap();
                Point destination = new(targetUser.X, targetUser.Y);
                for (int step = 1; step <= 3; step++) {
                    Point tile = new(targetUser.X + dx * step, targetUser.Y + dy * step);
                    bool blocked = !map.SquareIsOpen(tile.X, tile.Y, false)
                        || map.GetCoordinatedItems(tile).Any(i => i.GetBaseItem()?.InteractionType == InteractionType.Arrow);
                    if (blocked)
                        break;
                    destination = tile;
                }

                if (destination.X == targetUser.X && destination.Y == targetUser.Y) {
                    session.SendWhisper("Oops, there's no room to push " + @params[1] + " that way!", 1);
                    return;
                }

                targetUser.MoveTo(destination.X, destination.Y);

                room.SendPacket(new ShoutComposer(thisUser.VirtualId, "*super pushes " + @params[1] + " away from them*", 0, thisUser.LastBubble, isRpAction: true));
            } else {
                session.SendWhisper("Oops, " + @params[1] + " is not close enough!", 1);
            }
        }
    }
}