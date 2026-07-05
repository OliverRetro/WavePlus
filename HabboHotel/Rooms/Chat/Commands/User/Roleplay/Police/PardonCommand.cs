using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Police;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Commands;
using Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Police
{
    internal class PardonCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_hit";
        public string Parameters => "%username%";
        public string Description => "Clear a user's active wanted crimes (police only).";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 2) {
                session.SendWhisper("Usage: :pardon <username>", 1);
                return;
            }

            if (!PoliceManager.HasJobPermission(session.GetHabbo(), "rp.police.corporation.id", "pardon_user")) {
                session.SendWhisper("Only on-duty police officers can pardon suspects.", 1);
                return;
            }

            if (!RpCommandUtil.TryGetTarget(session, room, @params[1], out GameClient targetClient))
                return;

            PlusEnvironment.GetRpCrimeManager().ResolveAll(targetClient.GetHabbo().Id);
            PlusEnvironment.GetPoliceManager()?.Uncuff(targetClient.GetHabbo());

            targetClient.SendWhisper("Your active crimes have been pardoned by " + session.GetHabbo().Username + ".", 1);
            session.SendWhisper("You pardoned " + targetClient.GetHabbo().Username + ".", 1);
            RpCommandUtil.RoomAction(room, session.GetHabbo(), "*" + session.GetHabbo().Username + " pardons " + targetClient.GetHabbo().Username + "*");
        }
    }
}
