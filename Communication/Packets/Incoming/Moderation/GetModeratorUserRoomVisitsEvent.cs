using System;
using System.Collections.Generic;
using System.Data;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Database.Interfaces;
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
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `room_id`, `entry_timestamp` FROM `user_roomvisits` WHERE `user_id` = @id ORDER BY `entry_timestamp` DESC LIMIT 50");
                dbClient.AddParameter("id", userId);
                DataTable table = dbClient.GetTable();

                if (table != null) {
                    foreach (DataRow row in table.Rows)
                    {
                        if (!RoomFactory.TryGetData(Convert.ToInt32(row["room_id"]), out RoomData data))
                            continue;

                        if (!visits.ContainsKey(Convert.ToDouble(row["entry_timestamp"])))
                            visits.Add(Convert.ToDouble(row["entry_timestamp"]), data);
                    }
                }
            }

            session.SendPacket(new ModeratorUserRoomVisitsComposer(target.GetHabbo(), visits));
        }
    }
}