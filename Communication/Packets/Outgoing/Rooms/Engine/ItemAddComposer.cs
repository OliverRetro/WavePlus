using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class ItemAddComposer : MessageComposer
    {
        public Item Item { get; }

        public ItemAddComposer(Item item)
            : base(ServerPacketHeader.ItemAddMessageComposer)
        {
            Item = item;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteString(Item.Id.ToString());
            packet.WriteInteger(Item.GetBaseItem().SpriteId);
            packet.WriteString(Item.WallCoord != null ? Item.WallCoord : string.Empty);

            ItemBehaviourUtility.GenerateWallExtradata(Item, packet);

            packet.WriteInteger(-1);
            packet.WriteInteger(Item.GetBaseItem().IsUsable); // Type New R63 ('use bottom')
            packet.WriteInteger(Item.UserId);
            packet.WriteString(Item.Username);
        }
    }
}