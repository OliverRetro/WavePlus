using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Police;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Commands;
using Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Police
{
    internal class ArrestCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_hit";
        public string Parameters => "%username%";
        public string Description => "Arrest an adjacent suspect with active crimes (police only).";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room == null)
                return;

            if (@params.Length < 2) {
                session.SendWhisper("Usage: :arrest <username>", 1);
                return;
            }

            if (!PoliceManager.HasJobPermission(session.GetHabbo(), "rp.police.corporation.id", "arrest_user")) {
                session.SendWhisper("Only on-duty police officers can arrest suspects.", 1);
                return;
            }

            if (!RpCommandUtil.TryGetTarget(session, room, @params[1], out GameClient targetClient))
                return;

            if (!RpCommandUtil.RequireAdjacent(session, room, targetClient.GetHabbo(), "You need to be beside the suspect to arrest them."))
                return;

            int targetId = targetClient.GetHabbo().Id;

            PlusEnvironment.GetRpCrimeManager().RevealAll(targetId);
            int stars = PlusEnvironment.GetRpCrimeManager().GetTotalStars(targetId);
            if (stars <= 0) {
                session.SendWhisper(targetClient.GetHabbo().Username + " has no active crimes to arrest for.", 1);
                return;
            }

            bool jailed = PlusEnvironment.GetJailManager().Jail(targetClient.GetHabbo(), stars);
            PlusEnvironment.GetRpCrimeManager().ResolveAll(targetId);

            if (targetClient.GetHabbo().GetRpStats() != null) {
                targetClient.GetHabbo().GetRpStats().Arrested++;
                targetClient.GetHabbo().SaveRpStats();
                targetClient.GetHabbo().TrySendUserStatsUpdate(true);
            }

            PlusEnvironment.GetPoliceManager()?.Uncuff(targetClient.GetHabbo());

            string result = "Arrested " + targetClient.GetHabbo().Username + " with " + stars + " wanted star(s).";
            if (jailed)
                result += " They have been jailed.";
            session.SendWhisper(result, 1);
            targetClient.SendWhisper("You were arrested by " + session.GetHabbo().Username + ".", 1);
            RpCommandUtil.RoomAction(room, session.GetHabbo(), "*" + session.GetHabbo().Username + " arrests " + targetClient.GetHabbo().Username + "*");
        }
    }
}
