using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class ItemsComposer : MessageComposer
    {
        public Item[] Objects { get; }
        public int OwnerId { get; }
        public string OwnerName { get; }

        public ItemsComposer(Item[] objects, Room room)
            : base(ServerPacketHeader.ItemsMessageComposer)
        {
            Objects = objects;
            OwnerId = room.OwnerId;
            OwnerName = room.OwnerName;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(1);
            packet.WriteInteger(OwnerId);
            packet.WriteString(OwnerName);

            packet.WriteInteger(Objects.Length);

            foreach (Item item in Objects) {
                WriteWallItem(item, OwnerId, packet);
            }
        }

        private void WriteWallItem(Item item, int userId, ServerPacket packet)
        {
            packet.WriteString(item.Id.ToString());
            packet.WriteInteger(item.Data.SpriteId);

            try {
                packet.WriteString(item.WallCoord);
            }
            catch {
                packet.WriteString("");
            }

            ItemBehaviourUtility.GenerateWallExtradata(item, packet);

            packet.WriteInteger(-1);
            packet.WriteInteger((item.Data.Modes > 1) ? 1 : 0);
            packet.WriteInteger(userId);
        }
    }
}