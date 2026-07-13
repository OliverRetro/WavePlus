using System.Collections.Generic;
using Plus.HabboHotel.Catalog.Utilities;
using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Outgoing.Inventory.Furni
{
    internal class FurniListComposer : MessageComposer
    {
        public ICollection<Item> Items { get; }
        public int Pages { get; }
        public int Page { get; }

        public FurniListComposer(ICollection<Item> items, int pages, int page)
            : base(ServerPacketHeader.FurniListMessageComposer)
        {
            Items = items;
            Pages = pages;
            Page = page;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Pages); //Pages
            packet.WriteInteger(Page); //Page?

            packet.WriteInteger(Items.Count);
            foreach (Item item in Items)
            {
                WriteItem(item, packet);
            }
        }

        private void WriteItem(Item item, ServerPacket packet)
        {
            packet.WriteInteger(item.Id);
            packet.WriteString(item.GetBaseItem().Type.ToString().ToUpper());
            packet.WriteInteger(item.Id);
            packet.WriteInteger(item.GetBaseItem().SpriteId);

            if (item.LimitedNo > 0)
            {
                packet.WriteInteger(1);
                packet.WriteInteger(256);
                packet.WriteString(item.ExtraData);
                packet.WriteInteger(item.LimitedNo);
                packet.WriteInteger(item.LimitedTot);
            }
            else
                ItemBehaviourUtility.GenerateExtradata(item, packet);

            packet.WriteBoolean(item.GetBaseItem().AllowEcotronRecycle);
            packet.WriteBoolean(item.GetBaseItem().AllowTrade);
            packet.WriteBoolean(item.LimitedNo == 0 ? item.GetBaseItem().AllowInventoryStack : false);
            packet.WriteBoolean(ItemUtility.IsRare(item));
            packet.WriteInteger(-1); //Seconds to expiration.
            packet.WriteBoolean(true);
            packet.WriteInteger(-1); //Item RoomId

            if (!item.IsWallItem)
            {
                packet.WriteString(string.Empty);
                packet.WriteInteger(0);
            }
        }
    }
}