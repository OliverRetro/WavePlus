using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Cooldowns;
using Plus.HabboHotel.Roleplay.TargetLock;
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

            if (session.GetHabbo().GetRpStats().Energy < 1) {
                session.SendWhisper("You don't have enough energy!", 1);
                return;
            }

            if (PlusEnvironment.GetPoliceManager().IsCuffed(session.GetHabbo().Id)) {
                session.SendWhisper("You can't fight while cuffed.", 1);
                return;
            }

            RpCooldownManager cooldowns = PlusEnvironment.GetRpCooldownManager();
            int userId = session.GetHabbo().Id;

            if (cooldowns.IsReady(userId, RpCooldownKind.Combat) && PlusEnvironment.GetMayhemManager().TryHitBot(session.GetHabbo())) {
                session.GetHabbo().GetRpStats().InteractionEnergy();
                cooldowns.Apply(userId, RpCooldownKind.Combat);
                return;
            }

            if (!cooldowns.IsReady(userId, RpCooldownKind.Combat)) {
                session.SendWhisper("You must wait " + cooldowns.RemainingSecondsCeil(userId, RpCooldownKind.Combat) + " seconds to swing again.", 1);
                return;
            }

            GameClient targetClient;

            Habbo lockedTarget = TargetLockService.GetLockedTarget(session.GetHabbo());
            if (lockedTarget != null) {
                if (@params.Length >= 2 && !string.Equals(@params[1], lockedTarget.Username, StringComparison.OrdinalIgnoreCase)) {
                    session.SendWhisper("Target lock is active on " + lockedTarget.Username + ". Use :unlocktarget before attacking someone else.", 1);
                    return;
                }

                targetClient = lockedTarget.GetClient();
            } else if (@params.Length < 2) {
                Habbo clickedTarget = TargetLockService.GetTarget(session.GetHabbo());
                targetClient = clickedTarget?.GetClient();

                if (targetClient == null) {
                    session.SendWhisper("Enter a username, click a player, or use :locktarget <username>.", 1);
                    return;
                }
            } else {
                targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);
            }

            if (targetClient?.GetHabbo() == null) {
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

            if (session.GetHabbo().GetRpStats().PassiveMode || targetClient.GetHabbo().GetRpStats().PassiveMode || targetClient.GetHabbo().GetRpStats().IsGodMode) {
                session.SendWhisper("You cannot attack other users whilst either user is in passive mode.", 1);
                return;
            }

            // A swing that never landed (out of reach, target idle/down) costs a short beat rather
            // than the full 3s — otherwise one mistimed press against a moving target locks you
            // out of the fight for three seconds.
            bool swung = PlusEnvironment.GetCombatManager().AttackUser(session.GetHabbo(), targetClient.GetHabbo());
            cooldowns.Apply(userId, RpCooldownKind.Combat,
                RpCooldownManager.GetDefaultSeconds(swung ? RpCooldownKind.Combat : RpCooldownKind.CombatWhiff));
        }
    }
}