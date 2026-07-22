using System.Collections.Generic;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.Communication.Packets.Outgoing.Rooms.Settings;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    internal class RemoveAllRightsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
                return;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out Room instance))
                return;

            if (!instance.CheckRights(session, true))
                return;

            foreach (int userId in new List<int>(instance.UsersWithRights)) {
                RoomUser user = instance.GetRoomUserManager().GetRoomUserByHabbo(userId);
                if (user != null && !user.IsBot) {
                    user.RemoveStatus("flatctrl 1");
                    user.UpdateNeeded = true;

                    user.GetClient().SendPacket(new YouAreControllerComposer(0));
                }

                uint uid = (uint)userId;
                uint rid = (uint)instance.Id;
                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    db.RoomRights.Where(r => r.UserId == uid && r.RoomId == rid).ExecuteDelete();
                }

                session.SendPacket(new FlatControllerRemovedComposer(instance.Id, userId));
                session.SendPacket(new RoomRightsListComposer(instance));
                session.SendPacket(new UserUpdateComposer(instance.GetRoomUserManager().GetUserList().ToList()));
            }

            if (instance.UsersWithRights.Count > 0)
                instance.UsersWithRights.Clear();
        }
    }
}