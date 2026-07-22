using System.Collections.Generic;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Logs;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Moderation
{
    internal class GetModeratorRoomChatlogEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null)
                return;

            if (!session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                return;

            packet.PopInt(); //junk
            int roomId = packet.PopInt();

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(roomId, out Room room)) {
                return;
            }

            PlusEnvironment.GetGame().GetChatManager().GetLogs().FlushAndSave();

            List<ChatLogEntry> chats = new();

            uint rid = (uint)roomId;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var data = db.Chatlogs.Where(c => c.RoomId == rid)
                    .OrderByDescending(c => c.Id)
                    .Select(c => new { c.UserId, c.Message, c.Timestamp })
                    .Take(100)
                    .ToList();

                foreach (var row in data) {
                    Habbo habbo = PlusEnvironment.GetHabboById((int)row.UserId);

                    if (habbo != null) {
                        chats.Add(new ChatLogEntry((int)row.UserId, roomId, row.Message, row.Timestamp, habbo));
                    }
                }
            }

            session.SendPacket(new ModeratorRoomChatlogComposer(room, chats));
        }
    }
}