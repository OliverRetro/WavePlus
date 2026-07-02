using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;
using System;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay
{
    internal class HitCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_hit";

        public string Parameters => "%username%";

        public string Description => "Attack another user.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session == null || session.GetHabbo() == null || room == null || session?.GetHabbo().GetRpStats() == null)
                return;

            if(session.GetHabbo().GetRpStats().Energy < 1) {
                session.SendWhisper("You don't have enough energy!", 1);
                return;
            }

            double now = PlusEnvironment.GetUnixTimestamp();

            if (session.GetHabbo().LastHitTime > now) {
                double secondsLeft = session.GetHabbo().LastHitTime - now;

                session.SendWhisper("You must wait " + (int)Math.Ceiling(secondsLeft) + " seconds to swing again.", 1);
                return;
            }

            // The target username is supplied explicitly (":hit name") or auto-filled by
            // the keybind path (WebOverlayCallbackEvent.ExecuteMacro) from the user's target.
            if (@params.Length < 2) {
                session.SendWhisper("Enter a username, or use a keybind while targeting a user.", 1);
                return;
            }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);

            if (targetClient == null) {
                session.SendWhisper("The user could not be found.", 1);
                return;
            }

            if (targetClient == session) {
                session.SendWhisper("You cannot hit yourself!", 1);
                return;
            }
            
            if (targetClient.GetHabbo().CurrentRoomId != session.GetHabbo().CurrentRoomId) {
                session.SendWhisper("The user is not in the same room as you.", 1);
                return;
            }

            if (room.Safezone && !(targetClient.GetHabbo().GetRpStats().Aggression > 0 && session.GetHabbo().GetRpStats().Aggression > 0)) {
                session.SendWhisper("You cannot attack a user in a safezone, unless both of you have aggression.", 1);
                return;
            }

            if (session.GetHabbo().GetRpStats().PassiveMode || targetClient.GetHabbo().GetRpStats().PassiveMode) {
                session.SendWhisper("You cannot attack other users whilst either user is in passive mode.");
                return;
            }

            session.GetHabbo().LastHitTime = now + 3;
            PlusEnvironment.GetCombatManager().AttackUser(session.GetHabbo(), targetClient.GetHabbo());
        }
    }
}
