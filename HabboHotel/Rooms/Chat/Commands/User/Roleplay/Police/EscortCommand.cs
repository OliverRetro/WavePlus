using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Police
{
    internal class EscortCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_escort";
        public string GroupPermissionRequired => "escort_user";
        public string Parameters => "%username%";
        public string Description => "Escort a cuffed suspect (police only).";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 2) {
                session.SendWhisper("Usage: :escort <username>", 1);
                return;
            }

            if (!RpCommandUtil.TryGetTarget(session, room, @params[1], out GameClient targetClient))
                return;

            if (!PlusEnvironment.GetPoliceManager().IsCuffed(targetClient.GetHabbo().Id)) {
                session.SendWhisper("You must cuff the suspect before escorting them.", 1);
                return;
            }

            if (!RpCommandUtil.RequireAdjacent(session, room, targetClient.GetHabbo(), "You need to be beside the suspect to escort them."))
                return;

            PlusEnvironment.GetPoliceManager().StartArrestEscort(session.GetHabbo(), targetClient.GetHabbo(), 0);
            room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*starts escorting " + targetClient.GetHabbo().Username + "*", 0, 4, isRpAction: true));
        }
    }
}