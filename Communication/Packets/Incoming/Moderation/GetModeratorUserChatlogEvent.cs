using System.Collections.Generic;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Logs;
using Plus.HabboHotel.Users;
using Plus.Utilities;

namespace Plus.Communication.Packets.Incoming.Moderation
{
    internal class GetModeratorUserChatlogEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null)
                return;

            if (!session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                return;

            Habbo data = PlusEnvironment.GetHabboById(packet.PopInt());
            if (data == null) {
                session.SendNotification("Unable to load info for user.");
                return;
            }

            PlusEnvironment.GetGame().GetChatManager().GetLogs().FlushAndSave();

            List<KeyValuePair<RoomData, List<ChatLogEntry>>> chatlogs = new();

            uint uid = (uint)data.Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var getLogs = db.user_roomvisits.Where(v => v.user_id == uid)
                    .OrderByDescending(v => v.entry_timestamp)
                    .Select(v => new { v.room_id, v.entry_timestamp, v.exit_timestamp })
                    .Take(7)
                    .ToList();

                foreach (var row in getLogs)
                {
                    if (!RoomFactory.TryGetData((int)row.room_id, out RoomData roomData))
                        continue;

                    double timestampExit = (row.exit_timestamp <= 0 ? UnixTimestamp.GetNow() : row.exit_timestamp);

                    chatlogs.Add(new KeyValuePair<RoomData, List<ChatLogEntry>>(roomData, GetChatlogs(roomData, row.entry_timestamp, timestampExit)));
                }
            }

            session.SendPacket(new ModeratorUserChatlogComposer(data, chatlogs));
        }

        private List<ChatLogEntry> GetChatlogs(RoomData roomData, double timeEnter, double timeExit)
        {
            List<ChatLogEntry> chats = new();

            uint rid = (uint)roomData.Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var data = db.chatlogs.Where(c => c.room_id == rid && c.timestamp > timeEnter && c.timestamp < timeExit)
                    .OrderByDescending(c => c.timestamp)
                    .Select(c => new { c.user_id, c.timestamp, c.message })
                    .Take(100)
                    .ToList();

                foreach (var row in data)
                {
                    Habbo habbo = PlusEnvironment.GetHabboById((int)row.user_id);

                    if (habbo != null) {
                        chats.Add(new ChatLogEntry((int)row.user_id, roomData.Id, row.message, row.timestamp, habbo));
                    }
                }
            }

            return chats;
        }
    }
}