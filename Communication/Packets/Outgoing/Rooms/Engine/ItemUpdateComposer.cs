using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class ItemUpdateComposer : MessageComposer
    {
        public Item Item { get; }
        public int UserId { get; }

        public ItemUpdateComposer(Item item, int userId)
            : base(ServerPacketHeader.ItemUpdateMessageComposer)
        {
            Item = item;
            UserId = userId;
        }

        public override void Compose(ServerPacket packet)
        {
            WriteWallItem(Item, UserId, packet);
        }

        private void WriteWallItem(Item item, int userId, ServerPacket packet)
        {
            packet.WriteString(item.Id.ToString());
            packet.WriteInteger(item.GetBaseItem().SpriteId);
            packet.WriteString(item.WallCoord);
            switch (item.GetBaseItem().InteractionType) {
                case InteractionType.PostIt:
                    packet.WriteString(item.ExtraData.Split(' ')[0]);
                    break;

                default:
                    packet.WriteString(item.ExtraData);
                    break;
            }

            packet.WriteInteger(-1);
            packet.WriteInteger((item.GetBaseItem().Modes > 1) ? 1 : 0);
            packet.WriteInteger(userId);
        }
    }
}