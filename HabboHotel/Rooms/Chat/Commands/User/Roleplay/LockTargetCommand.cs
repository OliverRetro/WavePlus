using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.TargetLock;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay
{
    internal class LockTargetCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_locktarget";
        public string Parameters => "%username%";
        public string Description => "Lock your current RP target so swings only hit that player.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room == null)
                return;

            Habbo target = null;

            if (@params.Length >= 2) {
                // Resolved by name across the whole hotel: a lock does not require the
                // target to share your room, so RpCommandUtil.TryGetTarget's room check
                // (which is right for :hit, :rob, cuffing) must not apply here.
                string username = @params[1];

                target = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(username)?.GetHabbo();

                if (target == null) {
                    session.SendWhisper("That user could not be found.", 1);
                    return;
                }

                if (target.Id == session.GetHabbo().Id) {
                    session.SendWhisper("You cannot lock onto yourself.", 1);
                    return;
                }
            } else {
                target = TargetLockService.GetTarget(session.GetHabbo());
            }

            if (target == null) {
                session.SendWhisper("Click a player or use :locktarget <username>.", 1);
                return;
            }

            TargetLockService.LockTarget(session.GetHabbo(), target);
        }
    }
}