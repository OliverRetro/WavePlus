namespace Plus.Communication.Packets.Outgoing.Rooms.Session
{
    internal class RoomReadyComposer : MessageComposer
    {
        public int RoomId { get; }
        public string Model { get; }

        public RoomReadyComposer(int roomId, string model)
            : base(ServerPacketHeader.RoomReadyMessageComposer)
        {
            RoomId = roomId;
            Model = model;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteString(Model);
            packet.WriteInteger(RoomId);
        }
    }
}