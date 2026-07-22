using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Outgoing.Moderation
{
    internal class ModeratorRoomInfoComposer : MessageComposer
    {
        public RoomData Data { get; }
        public bool OwnerInRoom { get; }

        public ModeratorRoomInfoComposer(RoomData data, bool ownerInRoom)
            : base(ServerPacketHeader.ModeratorRoomInfoMessageComposer)
        {
            Data = data;
            OwnerInRoom = ownerInRoom;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Data.Id);
            packet.WriteInteger(Data.UsersNow);
            packet.WriteBoolean(OwnerInRoom); // owner in room
            packet.WriteInteger(Data.OwnerId);
            packet.WriteString(Data.OwnerName);
            packet.WriteBoolean(Data != null);
            packet.WriteString(Data.Name);
            packet.WriteString(Data.Description);

            packet.WriteInteger(Data.Tags.Count);
            foreach (string tag in Data.Tags) {
                packet.WriteString(tag);
            }

            packet.WriteBoolean(false);
        }
    }
}