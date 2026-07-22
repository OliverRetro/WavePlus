using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    internal class MimicCommand : IChatCommand
    {
        public string PermissionRequired => "command_mimic";

        public string Parameters => "%username%";

        public string Description => "Liking someone elses swag? Copy it!";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1) {
                session.SendWhisper("Please enter the username of the user you wish to mimic.");
                return;
            }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);
            if (targetClient == null) {
                session.SendWhisper("An error occoured whilst finding that user, maybe they're not online.");
                return;
            }

            if (!targetClient.GetHabbo().AllowMimic) {
                session.SendWhisper("Oops, you cannot mimic this user - sorry!");
                return;
            }

            RoomUser targetUser = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(targetClient.GetHabbo().Id);
            if (targetUser == null) {
                session.SendWhisper("An error occoured whilst finding that user, maybe they're not online or in this room.");
                return;
            }

            session.GetHabbo().Gender = targetUser.GetClient().GetHabbo().Gender;
            session.GetHabbo().Look = targetUser.GetClient().GetHabbo().Look;

            int userId = session.GetHabbo().Id;
            string gender = session.GetHabbo().Gender;
            string look = session.GetHabbo().Look;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.Users.Where(u => u.Id == userId).ExecuteUpdate(s => s.SetProperty(u => u.Gender, gender).SetProperty(u => u.Look, look));
            }

            RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user != null) {
                session.SendPacket(new AvatarAspectUpdateComposer(session.GetHabbo().Look, session.GetHabbo().Gender));
                session.SendPacket(new UserChangeComposer(user, true));
                room.SendPacket(new UserChangeComposer(user, false));
            }
        }
    }
}