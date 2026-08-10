namespace Plus.Communication.Packets.Outgoing.Roleplay
{
    internal class TeleportDissolveComposer : MessageComposer
    {
        private readonly int _virtualId;
        private readonly bool _rebuild;

        public TeleportDissolveComposer(int virtualId, bool rebuild = true)
            : base(ServerPacketHeader.TeleportDissolveMessageComposer)
        {
            _virtualId = virtualId;
            _rebuild = rebuild;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(_virtualId);
            packet.WriteBoolean(_rebuild);
        }
    }
}