using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Police;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Commands;
using Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Police
{
    internal class EscortCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_hit";
        public string Parameters => "%username%";
        public string Description => "Escort a cuffed suspect (police only).";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 2) {
                session.SendWhisper("Usage: :escort <username>", 1);
                return;
            }

            if (!PoliceManager.HasJobPermission(session.GetHabbo(), "rp.police.corporation.id", "escort_user")) {
                session.SendWhisper("Only on-duty police officers can escort suspects.", 1);
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

            PlusEnvironment.GetPoliceManager().StartEscort(session.GetHabbo(), targetClient.GetHabbo());
            session.SendWhisper("You are now escorting " + targetClient.GetHabbo().Username + ".", 1);
            targetClient.SendWhisper("You are being escorted by " + session.GetHabbo().Username + ".", 1);
            RpCommandUtil.RoomAction(room, session.GetHabbo(), "*" + session.GetHabbo().Username + " starts escorting " + targetClient.GetHabbo().Username + "*");
        }
    }
}
