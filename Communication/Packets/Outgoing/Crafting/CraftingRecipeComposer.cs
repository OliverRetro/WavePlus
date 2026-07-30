using System.Collections.Generic;

namespace Plus.Communication.Packets.Outgoing.Crafting
{
    internal class CraftingRecipeComposer : MessageComposer
    {
        private readonly List<(int Amount, string Key, string DisplayName, string IconUrl)> _ingredients;

        public CraftingRecipeComposer(List<(int Amount, string Key, string DisplayName, string IconUrl)> ingredients)
            : base(ServerPacketHeader.CraftingRecipeMessageComposer)
        {
            _ingredients = ingredients;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(_ingredients.Count);
            foreach (var (Amount, Key, DisplayName, IconUrl) in _ingredients) {
                packet.WriteInteger(Amount);
                packet.WriteString(Key);
                packet.WriteString(DisplayName);
                packet.WriteString(IconUrl);
            }
        }
    }
}