using System;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    internal class PullCommand : IChatCommand
    {
        public string PermissionRequired => "command_pull";

        public string Parameters => "%target%";

        public string Description => "Pull another user towards you.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1) {
                session.SendWhisper("Please enter the username of the user you wish to pull.");
                return;
            }

            if (!room.PullEnabled && !session.GetHabbo().GetPermissions().HasRight("room_override_custom_config")) {
                session.SendWhisper("Oops, it appears that the room owner has disabled the ability to use the pull command in here.");
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

            if (targetUser.TeleportEnabled) {
                session.SendWhisper("Oops, you cannot push a user whilst they have their teleport mode enabled.");
                return;
            }

            RoomUser thisUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (thisUser == null)
                return;

            if (thisUser.SetX - 1 == room.GetGameMap().Model.DoorX) {
                session.SendWhisper("Please don't pull that user out of the room :(!");
                return;
            }


            string pushDirection = "down";
            if (targetClient.GetHabbo().CurrentRoomId == session.GetHabbo().CurrentRoomId && (Math.Abs(thisUser.X - targetUser.X) < 3 && Math.Abs(thisUser.Y - targetUser.Y) < 3)) {
                room.SendPacket(new ShoutComposer(thisUser.VirtualId, "*" + session.GetHabbo().Username + " pulls " + @params[1] + " to them*", 0, thisUser.LastBubble));

                if (thisUser.RotBody == 0)
                    pushDirection = "up";
                if (thisUser.RotBody == 2)
                    pushDirection = "right";
                if (thisUser.RotBody == 4)
                    pushDirection = "down";
                if (thisUser.RotBody == 6)
                    pushDirection = "left";

                if (pushDirection == "up")
                    targetUser.MoveTo(thisUser.X, thisUser.Y - 1);

                if (pushDirection == "right")
                    targetUser.MoveTo(thisUser.X + 1, thisUser.Y);

                if (pushDirection == "down")
                    targetUser.MoveTo(thisUser.X, thisUser.Y + 1);

                if (pushDirection == "left")
                    targetUser.MoveTo(thisUser.X - 1, thisUser.Y);
                return;
            }

            session.SendWhisper("That user is not close enough to you to be pulled, try getting closer!");
        }
    }
}
