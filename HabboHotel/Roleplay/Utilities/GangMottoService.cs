using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database.EF;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Roleplay.Utilities
{
    public static class GangMottoService
    {
        public const string CitizenMotto = "Citizen";

        public static string Tag(GroupKind kind) => kind switch
        {
            GroupKind.Gang => "[GANG]",
            GroupKind.Mafia => "[MOB]",
            GroupKind.Cartel => "[CARTEL]",
            _ => "[GANG]"
        };

        public static void ApplyGangMotto(int userId, Group gang)
        {
            if (gang == null)
                return;

            SetBaseMotto(userId, Tag(gang.Kind) + " " + gang.Name);
        }

        public static void ClearToCitizen(int userId)
        {
            SetBaseMotto(userId, CitizenMotto);
        }

        private static void SetBaseMotto(int userId, string motto)
        {
            Habbo habbo = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId)?.GetHabbo();

            if (habbo != null) {
                bool working = PlusEnvironment.GetGame().GetShiftManager().IsWorking(userId);
                if (working) {
                    habbo.OldMotto = motto;
                } else {
                    habbo.Motto = motto;
                    Broadcast(habbo);
                }
            }

            Persist(userId, motto);
        }

        private static void Broadcast(Habbo habbo)
        {
            if (!habbo.InRoom || habbo.CurrentRoom == null)
                return;

            RoomUser roomUser = habbo.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
            if (roomUser != null)
                habbo.CurrentRoom.SendPacket(new UserChangeComposer(roomUser, false));
        }

        private static void Persist(int userId, string motto)
        {
            using WavePlusContext db = PlusEnvironment.GetDbContext();
            db.Users.Where(u => u.Id == userId).ExecuteUpdate(s => s.SetProperty(u => u.Motto, motto));
        }
    }
}