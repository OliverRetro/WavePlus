using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class RegenMaps : IChatCommand
    {
        public string PermissionRequired => "command_regen_maps";

        public string Parameters => "";

        public string Description => "Is the game map of your room broken? Fix it with this command!";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (!room.CheckRights(session, true))
            {
                session.SendWhisper("Oops, only the owner of this room can run this command!");
                return;
            }

            room.GetGameMap().GenerateMaps();
            session.SendWhisper("Game map of this room successfully re-generated.");
        }
    }
}