using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
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
            if (@params.Length == 1) {
                session.SendWhisper("You must specify a room id!");
                return;
            }

            if (!int.TryParse(@params[1], out int roomId)) {
                session.SendWhisper("You must enter a valid room ID");
                return;
            }

            if (!RoomFactory.TryGetData(roomId, out RoomData data)) {
                session.SendWhisper("This room does not exist!");
                return;
            }

            Room current = session.GetHabbo().CurrentRoom;
            RoomUser user = current?.GetRoomUserManager()?.GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user != null)
                current.SendPacket(new ShoutComposer(user.VirtualId, "*request for a Staff Taxi to " + data.Name + "*", 0, 23, isRpAction: true));

            session.SendPacket(new RoomForwardComposer(roomId));
        }
    }
}