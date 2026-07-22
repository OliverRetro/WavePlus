using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Rooms.Settings;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    internal class RemoveRightsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
                return;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out Room room))
                return;

            if (!room.CheckRights(session, true))
                return;

            int amount = packet.PopInt();
            for (int i = 0; (i < amount && i <= 100); i++) {
                int userId = packet.PopInt();
                if (userId > 0 && room.UsersWithRights.Contains(userId)) {
                    RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(userId);
                    if (user != null && !user.IsBot) {
                        user.RemoveStatus("flatctrl 1");
                        user.UpdateNeeded = true;

                        user.GetClient().SendPacket(new YouAreControllerComposer(0));
                    }

                    uint uid = (uint)userId;
                    uint rid = (uint)room.Id;
                    using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                        db.RoomRights.Where(r => r.UserId == uid && r.RoomId == rid).ExecuteDelete();
                    }

                    if (room.UsersWithRights.Contains(userId))
                        room.UsersWithRights.Remove(userId);

                    session.SendPacket(new FlatControllerRemovedComposer(room.Id, userId));
                }
            }
        }
    }
}