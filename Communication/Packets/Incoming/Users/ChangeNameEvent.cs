using System.Linq;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Users
{
    internal class ChangeNameEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null)
                return;

            Room room = session.GetHabbo().CurrentRoom;

            RoomUser user = room?.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Username);
            if (user == null)
                return;

            string newName = packet.PopString();
            string oldName = session.GetHabbo().Username;

            if (newName == oldName) {
                session.GetHabbo().ChangeName(oldName);
                session.SendPacket(new UpdateUsernameComposer(newName));
                return;
            }

            if (!CanChangeName(session.GetHabbo())) {
                session.SendNotification("Oops, it appears you currently cannot change your username!");
                return;
            }

            bool inUse;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                inUse = db.Users.Any(u => u.Username == newName);
            }

            char[] letters = newName.ToLower().ToCharArray();
            const string allowedCharacters = "abcdefghijklmnopqrstuvwxyz.,_-;:?!1234567890";

            if (letters.Any(chr => !allowedCharacters.Contains(chr)))
                return;

            if (!session.GetHabbo().GetPermissions().HasRight("mod_tool") && newName.ToLower().Contains("mod") || newName.ToLower().Contains("adm") || newName.ToLower().Contains("admin")
                || newName.ToLower().Contains("m0d") || newName.ToLower().Contains("mob") || newName.ToLower().Contains("m0b"))
                return;

            if (!newName.ToLower().Contains("mod") && (session.GetHabbo().Rank == 2 || session.GetHabbo().Rank == 3))
                return;

            if (newName.Length > 15)
                return;

            if (newName.Length < 3)
                return;

            if (inUse)
                return;

            if (!PlusEnvironment.GetGame().GetClientManager().UpdateClientUsername(session, oldName, newName)) {
                session.SendNotification("Oops! An issue occoured whilst updating your username.");
                return;
            }

            session.GetHabbo().ChangingName = false;

            room.GetRoomUserManager().RemoveUserFromRoom(session, true);

            session.GetHabbo().ChangeName(newName);
            session.GetHabbo().GetMessenger().OnStatusChanged(true);

            session.SendPacket(new UpdateUsernameComposer(newName));
            room.SendPacket(new UserNameChangeComposer(room.Id, user.VirtualId, newName));

            Plus.Core.Persistence.LogBuffer.LogNameChange(session.GetHabbo().Id, newName, oldName);

            foreach (Room ownRooms in PlusEnvironment.GetGame().GetRoomManager().GetRooms().ToList()) {
                if (ownRooms == null || ownRooms.OwnerId != session.GetHabbo().Id || ownRooms.OwnerName == newName)
                    continue;

                ownRooms.OwnerName = newName;
                ownRooms.SendPacket(new RoomInfoUpdatedComposer(ownRooms.Id));
            }

            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_Name", 1);

            session.SendPacket(new RoomForwardComposer(room.Id));
        }

        private static bool CanChangeName(Habbo habbo)
        {
            if (habbo.Rank == 1 && habbo.VipRank == 0 && habbo.LastNameChange == 0)
                return true;
            if (habbo.Rank == 1 && habbo.VipRank == 1 && (habbo.LastNameChange == 0 || (PlusEnvironment.GetUnixTimestamp() + 604800) > habbo.LastNameChange))
                return true;
            if (habbo.Rank == 1 && habbo.VipRank == 2 && (habbo.LastNameChange == 0 || (PlusEnvironment.GetUnixTimestamp() + 86400) > habbo.LastNameChange))
                return true;
            if (habbo.Rank == 1 && habbo.VipRank == 3)
                return true;
            if (habbo.GetPermissions().HasRight("mod_tool"))
                return true;

            return false;
        }
    }
}