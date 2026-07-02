using System;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay
{
    /// <summary>
    /// `:charge &lt;username&gt;` — an on-duty police officer charges an adjacent suspect. This
    /// reveals their hidden (non-auto-reported) crimes too, then jails them automatically.
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

            // Charger must be an on-duty police officer.
            int policeCorp = int.TryParse(PlusEnvironment.GetSettingsManager().TryGetValue("rp.police.corporation.id"), out int pc) ? pc : 0;
            if (policeCorp <= 0 || !PlusEnvironment.GetGame().GetShiftManager().IsWorkingFor(session.GetHabbo().Id, policeCorp)) {
                session.SendWhisper("Only on-duty police officers can charge suspects.", 1);
                return;
            }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);
            if (targetClient?.GetHabbo() == null) {
                session.SendWhisper("That user could not be found.", 1);
                return;
            }

            if (targetClient == session) {
                session.SendWhisper("You cannot charge yourself.", 1);
                return;
            }

            if (targetClient.GetHabbo().CurrentRoomId != session.GetHabbo().CurrentRoomId) {
                session.SendWhisper("The suspect is not in the same room as you.", 1);
                return;
            }

            RoomUser officer = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            RoomUser suspect = room.GetRoomUserManager().GetRoomUserByHabbo(targetClient.GetHabbo().Id);
            if (officer == null || suspect == null)
                return;

            if (Math.Abs(officer.X - suspect.X) + Math.Abs(officer.Y - suspect.Y) > 1) {
                session.SendWhisper("You need to be right next to the suspect to charge them.", 1);
                return;
            }

            int targetId = targetClient.GetHabbo().Id;

            // Charging exposes hidden crimes too.
            PlusEnvironment.GetRpCrimeManager().RevealAll(targetId);
            int stars = PlusEnvironment.GetRpCrimeManager().GetTotalStars(targetId);
            if (stars <= 0) {
                session.SendWhisper($"{targetClient.GetHabbo().Username} has no active crimes to charge.", 1);
                return;
            }

            bool jailed = PlusEnvironment.GetJailManager().Jail(targetClient.GetHabbo(), stars);

            // Crimes are resolved once charged (whether or not a jail term applied).
            PlusEnvironment.GetRpCrimeManager().ResolveAll(targetId);

            session.SendWhisper($"Charged {targetClient.GetHabbo().Username} with {stars} star(s)." + (jailed ? " They've been jailed." : ""), 1);
        }
    }
}
