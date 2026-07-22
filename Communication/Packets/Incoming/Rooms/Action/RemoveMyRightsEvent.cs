using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    internal class RemoveMyRightsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
                return;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out Room room))
                return;

            if (!room.CheckRights(session, false))
                return;

            if (room.UsersWithRights.Contains(session.GetHabbo().Id)) {
                RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
                if (user != null && !user.IsBot) {
                    user.RemoveStatus("flatctrl 1");
                    user.UpdateNeeded = true;

                    user.GetClient().SendPacket(new YouAreNotControllerComposer());
                }

                uint uid = (uint)session.GetHabbo().Id;
                uint rid = (uint)room.Id;
                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    db.RoomRights.Where(r => r.UserId == uid && r.RoomId == rid).ExecuteDelete();
                }

                if (room.UsersWithRights.Contains(session.GetHabbo().Id))
                    room.UsersWithRights.Remove(session.GetHabbo().Id);
            }
        }
    }
}