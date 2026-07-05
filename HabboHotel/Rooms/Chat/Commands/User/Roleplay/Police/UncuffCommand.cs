using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Police;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Commands;
using Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Police
{
    internal class UncuffCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_hit";
        public string Parameters => "%username%";
        public string Description => "Uncuff a user (police only).";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 2) {
                session.SendWhisper("Usage: :uncuff <username>", 1);
                return;
            }

            if (!PoliceManager.HasJobPermission(session.GetHabbo(), "rp.police.corporation.id", "release_user")) {
                session.SendWhisper("Only on-duty police officers can release cuffs.", 1);
                return;
            }

            if (!RpCommandUtil.TryGetTarget(session, room, @params[1], out GameClient targetClient))
                return;

            PlusEnvironment.GetPoliceManager().Uncuff(targetClient.GetHabbo());
            targetClient.SendWhisper("Your cuffs have been removed.", 1);
            session.SendWhisper("You uncuffed " + targetClient.GetHabbo().Username + ".", 1);
            RpCommandUtil.RoomAction(room, session.GetHabbo(), "*" + session.GetHabbo().Username + " removes " + targetClient.GetHabbo().Username + "'s cuffs*");
        }
    }
}
