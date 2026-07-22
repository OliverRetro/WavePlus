using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Rooms.Settings;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Moderation
{
    internal class ModerateRoomEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                return;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(packet.PopInt(), out Room room))
                return;

            bool setLock = packet.PopInt() == 1;
            bool setName = packet.PopInt() == 1;
            bool kickAll = packet.PopInt() == 1;

            if (setName) {
                room.Name = "Inappropriate to Hotel Management";
                room.Description = "Inappropriate to Hotel Management";
            }

            if (setLock)
                room.Access = RoomAccess.Doorbell;

            if (room.Tags.Count > 0)
                room.ClearTags();

            if (room.HasActivePromotion)
                room.EndPromotion();

            int roomId = room.RoomId;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                // NOTE: legacy sets `state` = '1' even though the column is an enum('open','locked','password','invisible'); preserved as-is.
                if (setName && setLock)
                    db.Rooms.Where(r => r.Id == roomId).ExecuteUpdate(s => s
                        .SetProperty(r => r.Caption, "Inappropriate to Hotel Management")
                        .SetProperty(r => r.Description, "Inappropriate to Hotel Management")
                        .SetProperty(r => r.Tags, "")
                        .SetProperty(r => r.State, "1"));
                else if (setName)
                    db.Rooms.Where(r => r.Id == roomId).ExecuteUpdate(s => s
                        .SetProperty(r => r.Caption, "Inappropriate to Hotel Management")
                        .SetProperty(r => r.Description, "Inappropriate to Hotel Management")
                        .SetProperty(r => r.Tags, ""));
                else if (setLock)
                    db.Rooms.Where(r => r.Id == roomId).ExecuteUpdate(s => s
                        .SetProperty(r => r.State, "1")
                        .SetProperty(r => r.Tags, ""));
            }

            room.SendPacket(new RoomSettingsSavedComposer(room.RoomId));
            room.SendPacket(new RoomInfoUpdatedComposer(room.RoomId));

            if (kickAll) {
                foreach (RoomUser roomUser in room.GetRoomUserManager().GetUserList().ToList()) {
                    if (roomUser == null || roomUser.IsBot)
                        continue;

                    if (roomUser.GetClient() == null || roomUser.GetClient().GetHabbo() == null)
                        continue;

                    if (roomUser.GetClient().GetHabbo().Rank >= session.GetHabbo().Rank || roomUser.GetClient().GetHabbo().Id == session.GetHabbo().Id)
                        continue;

                    room.GetRoomUserManager().RemoveUserFromRoom(roomUser.GetClient(), true);
                }
            }
        }
    }
}