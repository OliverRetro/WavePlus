using System.Collections.Generic;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Logs;
using Plus.HabboHotel.Users;
using Plus.Utilities;

namespace Plus.Communication.Packets.Outgoing.Moderation
{
    internal class ModeratorUserChatlogComposer : MessageComposer
    {
        public Habbo Habbo { get; }
        public List<KeyValuePair<RoomData, List<ChatLogEntry>>> ChatLogs { get; }

        public ModeratorUserChatlogComposer(Habbo habbo, List<KeyValuePair<RoomData, List<ChatLogEntry>>> chatlogs)
            : base(ServerPacketHeader.ModeratorUserChatlogMessageComposer)
        {
            Habbo = habbo;
            ChatLogs = chatlogs;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Habbo.Id);
            packet.WriteString(Habbo.Username);

            packet.WriteInteger(ChatLogs.Count); // Room Visits Count
            foreach (KeyValuePair<RoomData, List<ChatLogEntry>> chatlog in ChatLogs)
            {
                packet.WriteByte(1);
                packet.WriteShort(2); //Count
                packet.WriteString("roomName");
                packet.WriteByte(2);
                packet.WriteString(chatlog.Key.Name); // room name
                packet.WriteString("roomId");
                packet.WriteByte(1);
                packet.WriteInteger(chatlog.Key.Id);

                packet.WriteShort(chatlog.Value.Count); // Chatlogs Count
                foreach (ChatLogEntry entry in chatlog.Value)
                {
                    string username = "NOT FOUND";
                    if (entry.PlayerNullable() != null)
                    {
                        username = entry.PlayerNullable().Username;
                    }

                    packet.WriteString(UnixTimestamp.FromUnixTimestamp(entry.Timestamp).ToShortTimeString());
                    packet.WriteInteger(entry.PlayerId); // UserId of message
                    packet.WriteString(username); // Username of message
                    packet.WriteString(!string.IsNullOrEmpty(entry.Message) ? entry.Message : "** user sent a blank message **"); // Message        
                    packet.WriteBoolean(Habbo.Id == entry.PlayerId);
                }
            }
        }
    }
}