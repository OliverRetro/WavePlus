using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay
{
    internal class ChargeCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_hit";

        public string Parameters => "%username%";

        public string Description => "Charge an adjacent suspect, activating their crimes (police only).";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room == null)
                return;

            if (@params.Length < 2)
            {
                session.SendWhisper("Please enter the username of the suspect you wish to charge.", 1);
                return;
            }

            if (!RpCommandUtil.TryGetTarget(session, room, @params[1], out GameClient targetClient))
                return;

            if (!RpCommandUtil.RequireAdjacent(session, room, targetClient.GetHabbo(), "You need to be right next to the suspect to charge them."))
                return;

            int targetId = targetClient.GetHabbo().Id;

            if (PlusEnvironment.GetRpCrimeManager().GetTotalStars(targetId) <= 0)
            {
                session.SendWhisper(targetClient.GetHabbo().Username + " has no active crimes to charge.", 1);
                return;
            }

            int stars = PlusEnvironment.GetRpCrimeManager().Charge(targetId);
            room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*" + session.GetHabbo().Username + " charges " + targetClient.GetHabbo().Username + " with " + stars + " stars*", 0, 4));
        }
    }
}
