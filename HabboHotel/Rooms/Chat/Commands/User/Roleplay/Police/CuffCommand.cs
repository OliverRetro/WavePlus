using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Police;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Commands;
using Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Police
{
    internal class CuffCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_hit";
        public string Parameters => "%username%";
        public string Description => "Cuff an adjacent suspect (police only).";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 2) {
                session.SendWhisper("Usage: :cuff <username>", 1);
                return;
            }

            if (!PoliceManager.HasJobPermission(session.GetHabbo(), "rp.police.corporation.id", "cuff_user")) {
                session.SendWhisper("Only on-duty police officers can cuff users.", 1);
                return;
            }

            if (!RpCommandUtil.TryGetTarget(session, room, @params[1], out GameClient targetClient))
                return;

            if (!RpCommandUtil.RequireAdjacent(session, room, targetClient.GetHabbo(), "You need to be beside the suspect to cuff them."))
                return;

            PlusEnvironment.GetPoliceManager().Cuff(session.GetHabbo(), targetClient.GetHabbo());
            targetClient.SendWhisper("You have been cuffed. You cannot walk until released.", 1);
            session.SendWhisper("You cuffed " + targetClient.GetHabbo().Username + ".", 1);
            RpCommandUtil.RoomAction(room, session.GetHabbo(), "*" + session.GetHabbo().Username + " cuffs " + targetClient.GetHabbo().Username + "*");
        }
    }
}
