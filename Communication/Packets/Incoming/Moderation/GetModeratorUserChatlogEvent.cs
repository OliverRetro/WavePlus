using System;
using System.Collections.Generic;
using System.Data;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Database.Interfaces;
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
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `room_id`,`entry_timestamp`,`exit_timestamp` FROM `user_roomvisits` WHERE `user_id` = '" + data.Id + "' ORDER BY `entry_timestamp` DESC LIMIT 7");
                DataTable getLogs = dbClient.GetTable();

                if (getLogs != null) {
                    foreach (DataRow row in getLogs.Rows)
                    {
                        if (!RoomFactory.TryGetData(Convert.ToInt32(row["room_id"]), out RoomData roomData))
                            continue;

                        double timestampExit = (Convert.ToDouble(row["exit_timestamp"]) <= 0 ? UnixTimestamp.GetNow() : Convert.ToDouble(row["exit_timestamp"]));

                        chatlogs.Add(new KeyValuePair<RoomData, List<ChatLogEntry>>(roomData, GetChatlogs(roomData, Convert.ToDouble(row["entry_timestamp"]), timestampExit)));
                    }
                }

                session.SendPacket(new ModeratorUserChatlogComposer(data, chatlogs));
            }
        }

        private List<ChatLogEntry> GetChatlogs(RoomData roomData, double timeEnter, double timeExit)
        {
            List<ChatLogEntry> chats = new();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `user_id`, `timestamp`, `message` FROM `chatlogs` WHERE `room_id` = " + roomData.Id + " AND `timestamp` > " + timeEnter + " AND `timestamp` < " + timeExit + " ORDER BY `timestamp` DESC LIMIT 100");
                var data = dbClient.GetTable();

                if (data != null) {
                    foreach (DataRow row in data.Rows)
                    {
                        Habbo habbo = PlusEnvironment.GetHabboById(Convert.ToInt32(row["user_id"]));

                        if (habbo != null) {
                            chats.Add(new ChatLogEntry(Convert.ToInt32(row["user_id"]), roomData.Id, Convert.ToString(row["message"]), Convert.ToDouble(row["timestamp"]), habbo));
                        }
                    }
                }
            }

            return chats;
        }
    }
}