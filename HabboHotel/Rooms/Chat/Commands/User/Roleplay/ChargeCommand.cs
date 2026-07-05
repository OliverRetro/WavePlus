using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Police;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay
{
    /// <summary>
    /// `:charge <username>` — alias of arrest that reveals hidden crimes and jails the suspect.
    /// </summary>
    internal class ChargeCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_hit";

        public string Parameters => "%username%";

        public string Description => "Charge an adjacent suspect with their crimes (police only).";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room == null)
                return;

            if (@params.Length < 2) {
                session.SendWhisper("Please enter the username of the suspect you wish to charge.", 1);
                return;
            }

            if (!PoliceManager.HasJobPermission(session.GetHabbo(), "rp.police.corporation.id", "arrest_user")) {
                session.SendWhisper("Only on-duty police officers can charge suspects.", 1);
                return;
            }

            if (!RpCommandUtil.TryGetTarget(session, room, @params[1], out GameClient targetClient))
                return;

            if (!RpCommandUtil.RequireAdjacent(session, room, targetClient.GetHabbo(), "You need to be right next to the suspect to charge them."))
                return;

            int targetId = targetClient.GetHabbo().Id;

            PlusEnvironment.GetRpCrimeManager().RevealAll(targetId);
            int stars = PlusEnvironment.GetRpCrimeManager().GetTotalStars(targetId);
            if (stars <= 0) {
                session.SendWhisper(targetClient.GetHabbo().Username + " has no active crimes to charge.", 1);
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

            session.SendWhisper("Charged " + targetClient.GetHabbo().Username + " with " + stars + " star(s)." + (jailed ? " They've been jailed." : ""), 1);
            targetClient.SendWhisper("You were charged by " + session.GetHabbo().Username + ".", 1);
            RpCommandUtil.RoomAction(room, session.GetHabbo(), "*" + session.GetHabbo().Username + " charges " + targetClient.GetHabbo().Username + "*");
        }
    }
}
