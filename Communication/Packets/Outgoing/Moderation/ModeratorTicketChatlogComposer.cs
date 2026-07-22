using Plus.HabboHotel.Moderation;
using Plus.HabboHotel.Rooms;
using Plus.Utilities;

namespace Plus.Communication.Packets.Outgoing.Moderation
{
    internal class ModeratorTicketChatlogComposer : MessageComposer
    {
        public ModerationTicket ModerationTicket { get; }
        public RoomData RoomData { get; }
        public double Timestamp { get; }

        public ModeratorTicketChatlogComposer(ModerationTicket ticket, RoomData roomData, double timestamp)
            : base(ServerPacketHeader.ModeratorTicketChatlogMessageComposer)
        {
            ModerationTicket = ticket;
            RoomData = roomData;
            Timestamp = timestamp;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(ModerationTicket.Id);
            packet.WriteInteger(ModerationTicket.Sender != null ? ModerationTicket.Sender.Id : 0);
            packet.WriteInteger(ModerationTicket.Reported != null ? ModerationTicket.Reported.Id : 0);
            packet.WriteInteger(RoomData.Id);

            packet.WriteByte(1);
            packet.WriteShort(2); //Count
            packet.WriteString("roomName");
            packet.WriteByte(2);
            packet.WriteString(RoomData.Name);
            packet.WriteString("roomId");
            packet.WriteByte(1);
            packet.WriteInteger(RoomData.Id);

            packet.WriteShort(ModerationTicket.ReportedChats.Count);
            foreach (string chat in ModerationTicket.ReportedChats) {
                packet.WriteString(UnixTimestamp.FromUnixTimestamp(Timestamp).ToShortTimeString());
                packet.WriteInteger(ModerationTicket.Id);
                packet.WriteString(ModerationTicket.Reported != null ? ModerationTicket.Reported.Username : "No username");
                packet.WriteString(chat);
                packet.WriteBoolean(false);
            }
        }
    }
}