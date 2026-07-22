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
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var getLogs = db.UserRoomvisits.Where(v => v.UserId == uid)
                    .OrderByDescending(v => v.EntryTimestamp)
                    .Select(v => new { v.RoomId, v.EntryTimestamp, v.ExitTimestamp })
                    .Take(7)
                    .ToList();

                foreach (var row in getLogs) {
                    if (!RoomFactory.TryGetData((int)row.RoomId, out RoomData roomData))
                        continue;

                    double timestampExit = (row.ExitTimestamp <= 0 ? UnixTimestamp.GetNow() : row.ExitTimestamp);

                    chatlogs.Add(new KeyValuePair<RoomData, List<ChatLogEntry>>(roomData, GetChatlogs(roomData, row.EntryTimestamp, timestampExit)));
                }
            }

            session.SendPacket(new ModeratorUserChatlogComposer(data, chatlogs));
        }

        public List<ChatLogEntry> GetChatlogs(RoomData roomData, double timeEnter, double timeExit)
        {
            List<ChatLogEntry> chats = [];

            uint rid = (uint)roomData.Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var data = db.Chatlogs.Where(c => c.RoomId == rid && c.Timestamp > timeEnter && c.Timestamp < timeExit)
                    .OrderByDescending(c => c.Timestamp)
                    .Select(c => new { c.UserId, c.Timestamp, c.Message })
                    .Take(100)
                    .ToList();

                foreach (var row in data) {
                    Habbo habbo = PlusEnvironment.GetHabboById((int)row.UserId);

                    if (habbo != null) {
                        chats.Add(new ChatLogEntry((int)row.UserId, roomData.Id, row.Message, row.Timestamp, habbo));
                    }
                }
            }

            return chats;
        }
    }
}