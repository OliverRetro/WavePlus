using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.TargetLock;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay
{
    internal class UnlockTargetCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_unlocktarget";
        public string Parameters => "";
        public string Description => "Release your locked RP target.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null)
                return;

            TargetLockService.ReleaseLock(session.GetHabbo(), true);
        }
    }
}