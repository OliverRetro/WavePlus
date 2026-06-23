using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Corporation
{
    internal class StopWorkCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_stop_work";

        public string Parameters => "";

        public string Description => "Stop your work shift in the corporation.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room == null)
                return;

            PlusEnvironment.GetGame().GetShiftManager().TryStopWork(session, room);
        }
    }
}
