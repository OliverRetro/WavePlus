using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Police;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Commands;
using Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Police
{
    internal class StopEscortCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_hit";
        public string Parameters => "%username%";
        public string Description => "Stop escorting a suspect (police only).";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 2) {
                session.SendWhisper("Usage: :stopescort <username>", 1);
                return;
            }

            if (!PoliceManager.HasJobPermission(session.GetHabbo(), "rp.police.corporation.id", "unescort_user")) {
                session.SendWhisper("Only on-duty police officers can stop escorts.", 1);
                return;
            }

            if (!RpCommandUtil.TryGetTarget(session, room, @params[1], out GameClient targetClient))
                return;

            PlusEnvironment.GetPoliceManager().StopEscort(targetClient.GetHabbo().Id);
            session.SendWhisper("You stopped escorting " + targetClient.GetHabbo().Username + ".", 1);
            targetClient.SendWhisper("You are no longer being escorted.", 1);
        }
    }
}
