using System;
using Plus.HabboHotel.Items;
using Plus.Utilities;

namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class ObjectUpdateComposer : MessageComposer
    {
        public Item Item { get; }
        public int UserId { get; }

        public ObjectUpdateComposer(Item item, int userId)
            : base(ServerPacketHeader.ObjectUpdateMessageComposer)
        {
            Item = item;
            UserId = userId;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Item.Id);
            packet.WriteInteger(Item.GetBaseItem().SpriteId);
            packet.WriteInteger(Item.GetX);
            packet.WriteInteger(Item.GetY);
            packet.WriteInteger(Item.Rotation);
            packet.WriteString(string.Format("{0:0.00}", TextHandling.GetString(Item.GetZ)));
            packet.WriteString(string.Empty);

            if (Item.LimitedNo > 0) {
                packet.WriteInteger(1);
                packet.WriteInteger(256);
                packet.WriteString(Item.ExtraData);
                packet.WriteInteger(Item.LimitedNo);
                packet.WriteInteger(Item.LimitedTot);
            } else {
                ItemBehaviourUtility.GenerateExtradata(Item, packet);
            }

            packet.WriteInteger(-1); // to-do: check
            packet.WriteInteger((Item.GetBaseItem().Modes > 1) ? 1 : 0);
            packet.WriteInteger(UserId);
        }
    }
}