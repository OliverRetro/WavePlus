using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.Communication.Packets.Outgoing.Rooms.Settings;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.Cache.Type;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
// Disambiguates the bare name from the scaffolded EF entity Plus.Database.EF.Entities.Room.
using Room = Plus.HabboHotel.Rooms.Room;

namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    internal class AssignRightsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null)
                return;

            int userId = packet.PopInt();

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out Room room))
                return;

            if (!room.CheckRights(session, true))
                return;

            if (room.UsersWithRights.Contains(userId)) {
                session.SendNotification(PlusEnvironment.GetLanguageManager().TryGetValue("room.rights.user.has_rights"));
                return;
            }

            room.UsersWithRights.Add(userId);

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.RoomRights.Add(new RoomRightEntity { RoomId = (uint)room.RoomId, UserId = (uint)userId });
                db.SaveChanges();
            }

            RoomUser roomUser = room.GetRoomUserManager().GetRoomUserByHabbo(userId);
            if (roomUser != null && !roomUser.IsBot) {
                roomUser.SetStatus("flatctrl 1");
                roomUser.UpdateNeeded = true;
                roomUser.GetClient()?.SendPacket(new YouAreControllerComposer(1));

                session.SendPacket(new FlatControllerAddedComposer(room.RoomId, roomUser.GetClient().GetHabbo().Id, roomUser.GetClient().GetHabbo().Username));
            } else {
                UserCache user = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(userId);
                if (user != null)
                    session.SendPacket(new FlatControllerAddedComposer(room.RoomId, user.Id, user.Username));
            }
        }
    }
}