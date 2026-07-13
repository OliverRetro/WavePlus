namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class SlideObjectBundleComposer : MessageComposer
    {
        public int FromX { get; }
        public int FromY { get; }
        public double FromZ { get; }
        public int ToX { get; }
        public int ToY { get; }
        public double ToZ { get; }
        public int RollerId { get; }
        public int AvatarId { get; }
        public int ItemId { get; }

        public SlideObjectBundleComposer(int fromX, int fromY, double fromZ, int toX, int toY, double toZ, int rollerId, int avatarId, int itemId)
            : base(ServerPacketHeader.SlideObjectBundleMessageComposer)
        {
            FromX = fromX;
            FromY = fromY;
            FromZ = fromZ;
            ToX = toX;
            ToY = toY;
            ToZ = toZ;
            RollerId = rollerId;
            AvatarId = avatarId;
            ItemId = itemId;
        }

        public override void Compose(ServerPacket packet)
        {
            bool isItem = ItemId > 0;

            packet.WriteInteger(FromX);
            packet.WriteInteger(FromY);
            packet.WriteInteger(ToX);
            packet.WriteInteger(ToY);
            packet.WriteInteger(isItem ? 1 : 0);

            if (isItem)
                packet.WriteInteger(ItemId);
            else
            {
                packet.WriteInteger(RollerId);
                packet.WriteInteger(2);
                packet.WriteInteger(AvatarId);
            }

            packet.WriteDouble(FromZ);
            packet.WriteDouble(ToZ);

            if (isItem)
            {
                packet.WriteInteger(RollerId);
            }
        }
    }
}