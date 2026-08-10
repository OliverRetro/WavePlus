namespace Plus.Communication.Packets.Outgoing.Roleplay.Gang
{
    internal class RPGangNoticeComposer : MessageComposer
    {
        private readonly int _code;
        private readonly string _message;
        private readonly string _popupTitle;
        private readonly string _popupMessage;
        private readonly int _primary;
        private readonly int _secondary;

        public RPGangNoticeComposer(string message)
            : this(0, message, string.Empty, string.Empty, 0, 0)
        {
        }

        // primary/secondary are group-palette colour ids (colour1/colour2).
        public RPGangNoticeComposer(int code, string message, string popupTitle, string popupMessage, int primary, int secondary)
            : base(ServerPacketHeader.GangNoticeMessageComposer)
        {
            _code = code;
            _message = message ?? string.Empty;
            _popupTitle = popupTitle ?? string.Empty;
            _popupMessage = popupMessage ?? string.Empty;
            _primary = primary;
            _secondary = secondary;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(_code);
            packet.WriteString(_message);
            packet.WriteString(_popupTitle);
            packet.WriteString(_popupMessage);
            packet.WriteInteger(_primary);
            packet.WriteInteger(_secondary);
        }
    }
}