using System.Collections.Generic;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Moderation
{
    internal class GetModeratorUserRoomVisitsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                return;

            int userId = packet.PopInt();
            GameClient target = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
            if (target == null)
                return;

            Dictionary<double, RoomData> visits = new();
            uint uid = (uint)userId;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var table = db.UserRoomvisits.Where(v => v.UserId == uid)
                    .OrderByDescending(v => v.EntryTimestamp)
                    .Select(v => new { v.RoomId, v.EntryTimestamp })
                    .Take(50)
                    .ToList();

                foreach (var row in table) {
                    if (!RoomFactory.TryGetData((int)row.RoomId, out RoomData data))
                        continue;

                    if (!visits.ContainsKey(row.EntryTimestamp))
                        visits.Add(row.EntryTimestamp, data);
                }
            }

            session.SendPacket(new ModeratorUserRoomVisitsComposer(target.GetHabbo(), visits));
        }
    }
}