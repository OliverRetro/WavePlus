using System;
using System.Data;
using Plus.Database.Interfaces;

namespace Plus.Communication.Packets.Outgoing.Avatar
{
    internal class WardrobeComposer : MessageComposer
    {
        public int UserId { get; }

        public WardrobeComposer(int userId)
            : base(ServerPacketHeader.WardrobeMessageComposer)
        {
            UserId = userId;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(1);
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `slot_id`,`look`,`gender` FROM `user_wardrobe` WHERE `user_id` = '" + UserId + "'");
                DataTable wardrobeData = dbClient.GetTable();

                if (wardrobeData == null)
                    packet.WriteInteger(0);
                else {
                    packet.WriteInteger(wardrobeData.Rows.Count);
                    foreach (DataRow row in wardrobeData.Rows)
                    {
                        packet.WriteInteger(Convert.ToInt32(row["slot_id"]));
                        packet.WriteString(Convert.ToString(row["look"]));
                        packet.WriteString(row["gender"].ToString().ToUpper());
                    }
                }
            }
        }
    }
}