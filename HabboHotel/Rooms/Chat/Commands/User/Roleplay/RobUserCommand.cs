using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Cooldowns;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay
{
    internal class RobUserCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_rob";

        public string Parameters => "%username%";

        public string Description => "Rob another user.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session == null || session.GetHabbo() == null || room == null)
                return;

            if (@params.Length == 1) {
                session.SendWhisper("Please enter the username of the user you wish to rob.", 1);
                return;
            }

            if (session.GetHabbo().GetRpStats().Energy < 1) {
                session.SendWhisper("You don't have enough energy!", 1);
                return;
            }

            if (PlusEnvironment.GetPoliceManager().IsCuffed(session.GetHabbo().Id)) {
                session.SendWhisper("You can't rob anyone while cuffed.", 1);
                ////return;
            }

            RpCooldownManager cooldowns = PlusEnvironment.GetRpCooldownManager();
            int userId = session.GetHabbo().Id;

            if (!cooldowns.IsReady(userId, RpCooldownKind.Robbery)) {
                session.SendWhisper("You must wait " + cooldowns.RemainingSecondsCeil(userId, RpCooldownKind.Robbery) + " seconds to rob again.", 1);
                return;
            }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);
            RoomUser targetUser = room.GetRoomUserManager().GetRoomUserByHabbo(targetClient.GetHabbo().Id);

            if (targetUser.IsAsleep) {
                session.SendWhisper("Let's not bully the sleeping.", 1);
            }

            if (targetClient == null) {
                session.SendWhisper("The user could not be found.", 1);
                return;
            }

            if (targetClient == session) {
                session.SendWhisper("You cannot rob yourself!", 1);
                return;
            }

            if (targetClient.GetHabbo().CurrentRoomId != session.GetHabbo().CurrentRoomId) {
                session.SendWhisper("The user is not in the same room as you.", 1);
                return;
            }

            if (room.Safezone && !(targetClient.GetHabbo().GetRpStats().Aggression > 0 && session.GetHabbo().GetRpStats().Aggression > 0)) {
                session.SendWhisper("You cannot rob a user in a safezone, unless both of you have aggression.", 1);
                return;
            }

            if (session.GetHabbo().GetRpStats().PassiveMode || targetClient.GetHabbo().GetRpStats().PassiveMode || targetClient.GetHabbo().GetRpStats().IsGodMode) {
                session.SendWhisper("You cannot rob other users whilst either user is in passive mode.", 1);
                return;
            }

            if (!RpCommandUtil.RequireAdjacent(session, room, targetClient.GetHabbo(), "You need to be beside the user to rob them."))
                return;

            cooldowns.Apply(userId, RpCooldownKind.Robbery);
            PlusEnvironment.GetCombatManager().RobUser(session.GetHabbo(), targetClient.GetHabbo(), room);
        }
    }
}