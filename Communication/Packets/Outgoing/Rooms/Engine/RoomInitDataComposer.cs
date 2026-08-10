namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class RoomInitDataComposer : MessageComposer
    {
        private ServerPacket _packet;

        public RoomInitDataComposer(ServerPacket packet) : base(ServerPacketHeader.RoomInitDataMessageComposer)
        {
            this._packet = packet;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteBytes(this._packet.Buffer);
        }

    }
}