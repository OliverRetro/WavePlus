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
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var data = db.chatlogs.Where(c => c.room_id == rid)
                    .OrderByDescending(c => c.id)
                    .Select(c => new { c.user_id, c.message, c.timestamp })
                    .Take(100)
                    .ToList();

                foreach (var row in data)
                {
                    Habbo habbo = PlusEnvironment.GetHabboById((int)row.user_id);

                    if (habbo != null) {
                        chats.Add(new ChatLogEntry((int)row.user_id, roomId, row.message, row.timestamp, habbo));
                    }
                }
            }

            session.SendPacket(new ModeratorRoomChatlogComposer(room, chats));
        }
    }
}