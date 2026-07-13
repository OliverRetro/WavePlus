using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class DisableDiagonalCommand : IChatCommand
    {
        public string PermissionRequired => "command_disable_diagonal";

        public string Parameters => "";

        public string Description => "Want to disable diagonal walking in your room? Type this command!";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (!room.CheckRights(session, true))
            {
                session.SendWhisper("Oops, only the owner of this room can run this command!");
                return;
            }

            room.GetGameMap().DiagonalEnabled = !room.GetGameMap().DiagonalEnabled;
            session.SendWhisper("Successfully updated the diagonal boolean value for this room.");
        }
    }
}