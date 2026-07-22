using System.Linq;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Rcon.Commands.User
{
    internal class ReloadUserMottoCommand : IRconCommand
    {
        public string Description => "This command is used to reload the users motto from the database.";

        public string Parameters => "%userId%";

        public bool TryExecute(string[] parameters)
        {
            if (!int.TryParse(parameters[0], out int userId))
                return false;

            GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
            if (client == null || client.GetHabbo() == null)
                return false;

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                client.GetHabbo().Motto = db.Users.Where(u => u.Id == userId).Select(u => u.Motto).FirstOrDefault();
            }

            // If we're in a room, we cannot really send the packets, so flag this as completed successfully, since we already updated it.
            if (!client.GetHabbo().InRoom) {
                return true;
            }

            //We are in a room, let's try to run the packets.
            Room room = client.GetHabbo().CurrentRoom;
            RoomUser user = room?.GetRoomUserManager().GetRoomUserByHabbo(client.GetHabbo().Id);
            if (user != null) {
                room.SendPacket(new UserChangeComposer(user, false));
                return true;
            }

            return false;
        }
    }
}