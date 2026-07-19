using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class SuperFastwalkCommand : IChatCommand
    {
        public string PermissionRequired => "command_super_fastwalk";

        public string Parameters => "";

        public string Description => "Gives you the ability to walk very very fast.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
                return;

            user.SuperFastWalking = !user.SuperFastWalking;

            if (user.FastWalking)
                user.FastWalking = false;

            // Mirror onto the habbo so the mode survives room swaps.
            session.GetHabbo().SuperFastWalkingEnabled = user.SuperFastWalking;
            session.GetHabbo().FastWalkingEnabled = user.FastWalking;

            session.SendWhisper("Walking mode updated.");
        }
    }
}