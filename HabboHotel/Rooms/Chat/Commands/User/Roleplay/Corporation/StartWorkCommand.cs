using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Corporation
{
    internal class StartWorkCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_start_work";

        public string Parameters => "";

        public string Description => "Start your work shift in the corporation.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room == null || room?.Group == null)
                return;

            PlusEnvironment.GetGame().GetShiftManager().TryStartWork(session, room);
        }
    }
}
