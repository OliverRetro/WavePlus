using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class SendToCommand : IChatCommand
    {
        public string PermissionRequired => "command_sendto";

        public string Parameters => "%username% %room_id%";

        public string Description => "Send another user to a room.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 3) {
                session.SendWhisper("Please enter a username and a room id. Example: :sendto Oliver 12345");
                return;
            }

            if (!int.TryParse(@params[2], out int roomId)) {
                session.SendWhisper("You must enter a valid room ID.");
                return;
            }

            if (!RoomFactory.TryGetData(roomId, out RoomData data)) {
                session.SendWhisper("This room does not exist!");
                return;
            }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);
            if (targetClient == null || targetClient.GetHabbo() == null) {
                session.SendWhisper("An error occurred whilst finding that user, maybe they're not online.");
                return;
            }

            Room current = session.GetHabbo().CurrentRoom;
            RoomUser user = current?.GetRoomUserManager()?.GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user != null)
                current.SendPacket(new ShoutComposer(user.VirtualId, "*sends " + targetClient.GetHabbo().Username + " to " + data.Name + "*", 0, 23, isRpAction: true));

            targetClient.SendPacket(new RoomForwardComposer(roomId));
        }
    }
}