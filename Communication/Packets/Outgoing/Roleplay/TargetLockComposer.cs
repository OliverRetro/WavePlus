namespace Plus.Communication.Packets.Outgoing.Roleplay
{
    public class TargetLockComposer : MessageComposer
    {
        private readonly string _targetName;
        private readonly bool _isLocked;

        public TargetLockComposer(string targetName, bool isLocked)
            : base(ServerPacketHeader.TargetLockMessageComposer)
        {
            _targetName = targetName ?? string.Empty;
            _isLocked = isLocked;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteBoolean(_isLocked);
            packet.WriteString(_targetName);
        }
    }
}