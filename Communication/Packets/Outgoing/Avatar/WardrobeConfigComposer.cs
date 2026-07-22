namespace Plus.Communication.Packets.Outgoing.Avatar
{
    internal class WardrobeConfigComposer : MessageComposer
    {
        public WardrobeConfigComposer()
            : base(ServerPacketHeader.WardrobeConfigMessageComposer)
        {
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(GetRoomId("rp.wardrobe.room.body"));
            packet.WriteInteger(GetRoomId("rp.wardrobe.room.head"));
        }

        private static int GetRoomId(string key)
        {
            int.TryParse(PlusEnvironment.GetSettingsManager().TryGetValue(key), out int roomId);
            return roomId;
        }
    }
}