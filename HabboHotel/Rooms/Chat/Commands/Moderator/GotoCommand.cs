using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class GotoCommand : IChatCommand
    {
        public string PermissionRequired => "command_goto";

        public string Parameters => "%room_id%";

        public string Description => "";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1)
            {
                session.SendWhisper("You must specify a room id!");
                return;
            }


            if (!int.TryParse(@params[1], out int roomId))
                session.SendWhisper("You must enter a valid room ID");
            else
            {
                if (!RoomFactory.TryGetData(roomId, out RoomData _))
                {
                    session.SendWhisper("This room does not exist!");
                    return;
                }

                session.GetHabbo().PrepareRoom(roomId, "");
            }
        }
    }
}