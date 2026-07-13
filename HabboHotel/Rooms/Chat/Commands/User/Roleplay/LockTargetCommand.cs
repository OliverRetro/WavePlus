using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.TargetLock;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay
{
    internal class LockTargetCommand : IChatCommand
    {
        public string PermissionRequired => string.Empty;
        public string Parameters => "%username%";
        public string Description => "Lock your current RP target so swings only hit that player.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room == null)
                return;

            Habbo target = null;

            if (@params.Length >= 2)
            {
                if (!RpCommandUtil.TryGetTarget(session, room, @params[1], out GameClient targetClient))
                    return;

                target = targetClient.GetHabbo();
            }
            else
            {
                target = TargetLockService.GetTarget(session.GetHabbo());
            }

            if (target == null)
            {
                session.SendWhisper("Click a player or use :locktarget <username>.", 1);
                return;
            }

            TargetLockService.LockTarget(session.GetHabbo(), target);
        }
    }
}
