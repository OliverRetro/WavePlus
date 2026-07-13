namespace Plus.Communication.Packets.Outgoing.Rooms.Notifications
{
    internal class RoomNotificationComposer : MessageComposer
    {
        public string Type { get; }
        public string Key { get; }
        public string Value { get; }

        public string Title { get; }
        public string Message { get; }
        public string Image { get; }
        public string HotelName { get; }
        public string HotelUrl { get; }

        public RoomNotificationComposer(string type, string key, string value)
            : base(ServerPacketHeader.RoomNotificationMessageComposer)
        {
            Type = type;
            Key = key;
            Value = value;
        }

        public RoomNotificationComposer(string type)
            : base(ServerPacketHeader.RoomNotificationMessageComposer)
        {
            Type = type;
        }

        public RoomNotificationComposer(string title, string message, string image, string hotelName = "", string hotelUrl = "")
            : base(ServerPacketHeader.RoomNotificationMessageComposer)
        {
            Title = title;
            Message = message;
            Image = image;
            HotelName = hotelName;
            HotelUrl = hotelUrl;
        }

        public override void Compose(ServerPacket packet)
        {
            if (!string.IsNullOrEmpty(Message))
            {
                packet.WriteString(Image);
                packet.WriteInteger(string.IsNullOrEmpty(HotelName) ? 2 : 4);
                packet.WriteString("title");
                packet.WriteString(Title);
                packet.WriteString("message");
                packet.WriteString(Message);

                if (!string.IsNullOrEmpty(HotelName))
                {
                    packet.WriteString("linkUrl");
                    packet.WriteString(HotelUrl);
                    packet.WriteString("linkTitle");
                    packet.WriteString(HotelName);
                }
            }
            else if (!string.IsNullOrEmpty(Key))
            {
                packet.WriteString(Type);
                packet.WriteInteger(1); //Count
                {
                    packet.WriteString(Key); //Type of message
                    packet.WriteString(Value);
                }
            }
            else
            {
                packet.WriteString(Type);
                packet.WriteInteger(0); //Count
            }
        }
    }
}