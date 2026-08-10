using System.Collections.Generic;
using Plus.HabboHotel.Roleplay.Clothing;

namespace Plus.Communication.Packets.Outgoing.Roleplay.Clothing
{
    internal class ClothingStoreComposer : MessageComposer
    {
        private readonly IReadOnlyList<RpClothingCategory> _categories;

        public ClothingStoreComposer(IReadOnlyList<RpClothingCategory> categories)
            : base(ServerPacketHeader.ClothingStoreMessageComposer)
        {
            _categories = categories;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(_categories.Count);

            foreach (RpClothingCategory category in _categories) {
                packet.WriteInteger(category.Id);
                packet.WriteString(category.TabName);

                packet.WriteInteger(category.Sets.Count);
                foreach (RpClothingSet set in category.Sets) {
                    packet.WriteInteger(set.Id);
                    packet.WriteString(set.SetType);
                    packet.WriteString(set.Name);
                    packet.WriteString(set.PartTypesRaw);
                    packet.WriteString(set.ColorRaw);
                    packet.WriteInteger(set.Stock);
                    packet.WriteInteger(set.BasePrice);
                    packet.WriteInteger(set.DiscountPrice);
                    packet.WriteInteger(set.EffectivePrice);
                }
            }
        }
    }
}