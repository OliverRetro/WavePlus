using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay
{
    internal class SuicideCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_giveup";

        public string Parameters => "";

        public string Description => "Give up and go straight to hospital.";

        public bool UsableWhileDead => true;

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo()?.GetRpStats()?.IsDead != true) {
                session.SendWhisper("You are not bleeding out.", 1);
                return;
            }

            if (session.GetHabbo().GetRpStats().HospitalReleaseTime > 0) {
                session.SendWhisper("You are already on your way to the hospital.", 1);
                return;
            }

            PlusEnvironment.GetHospitalManager().SkipBleedout(session.GetHabbo());
        }
    }
}