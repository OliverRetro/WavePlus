using System.Collections.Generic;

namespace Plus.Communication.Packets.Outgoing.Rooms.Notifications
{
    internal class BubbleAlertComposer : MessageComposer
    {
        public string ErrorKey { get; }
        public Dictionary<string, string> Keys { get; }

        public BubbleAlertComposer(string errorKey, Dictionary<string, string> keys)
            : base(ServerPacketHeader.RoomNotificationMessageComposer)
        {
            ErrorKey = errorKey;
            Keys = keys ?? new Dictionary<string, string>();
        }

        public BubbleAlertComposer(string errorKey, string message)
            : this(errorKey, new Dictionary<string, string>
            {
                { "message", message }
            })
        {
        }

        public BubbleAlertComposer(string errorKey)
            : this(errorKey, new Dictionary<string, string>())
        {
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteString(ErrorKey);
            packet.WriteInteger(Keys.Count);

            foreach (var kvp in Keys) {
                packet.WriteString(kvp.Key);
                packet.WriteString(kvp.Value);
            }
        }
    }
}