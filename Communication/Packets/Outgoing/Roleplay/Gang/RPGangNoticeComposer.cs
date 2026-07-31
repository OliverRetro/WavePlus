namespace Plus.Communication.Packets.Outgoing.Roleplay.Gang
{
    internal class RPGangNoticeComposer : MessageComposer
    {
        private readonly int _code;
        private readonly string _message;
        private readonly string _popupTitle;
        private readonly string _popupMessage;
        private readonly string _primary;
        private readonly string _secondary;

        public RPGangNoticeComposer(string message)
            : this(0, message, string.Empty, string.Empty, string.Empty, string.Empty)
        {
        }

        public RPGangNoticeComposer(int code, string message, string popupTitle, string popupMessage, string primary, string secondary)
            : base(ServerPacketHeader.GangNoticeMessageComposer)
        {
            _code = code;
            _message = message ?? string.Empty;
            _popupTitle = popupTitle ?? string.Empty;
            _popupMessage = popupMessage ?? string.Empty;
            _primary = primary ?? string.Empty;
            _secondary = secondary ?? string.Empty;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(_code);
            packet.WriteString(_message);
            packet.WriteString(_popupTitle);
            packet.WriteString(_popupMessage);
            packet.WriteString(_primary);
            packet.WriteString(_secondary);
        }
    }
}
