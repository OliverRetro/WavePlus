using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Police;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Commands;
using Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Police
{
    internal class ReleasePrisonerCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_hit";
        public string Parameters => "%username%";
        public string Description => "Release a jailed or cuffed user (police only).";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 2) {
                session.SendWhisper("Usage: :release <username>", 1);
                return;
            }

            if (!PoliceManager.HasJobPermission(session.GetHabbo(), "rp.police.corporation.id", "release_user")) {
                session.SendWhisper("Only on-duty police officers can release prisoners.", 1);
                return;
            }

            if (!RpCommandUtil.TryGetTarget(session, room, @params[1], out GameClient targetClient))
                return;

            PlusEnvironment.GetPoliceManager()?.Uncuff(targetClient.GetHabbo());
            PlusEnvironment.GetJailManager().Release(targetClient.GetHabbo());

            targetClient.SendWhisper("You have been released by " + session.GetHabbo().Username + ".", 1);
            session.SendWhisper("You released " + targetClient.GetHabbo().Username + ".", 1);
            RpCommandUtil.RoomAction(room, session.GetHabbo(), "*" + session.GetHabbo().Username + " releases " + targetClient.GetHabbo().Username + "*");
        }
    }
}
