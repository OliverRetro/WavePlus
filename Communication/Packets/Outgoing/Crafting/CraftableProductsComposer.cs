using System.Collections.Generic;

namespace Plus.Communication.Packets.Outgoing.Crafting
{
    internal class CraftableProductsComposer : MessageComposer
    {
        private readonly List<(string Name, string Key, string DisplayName, string IconUrl)> _recipes;
        private readonly List<(string Key, string DisplayName, string IconUrl, int Count)> _pool;
        private readonly int _craftSeconds;
        private readonly bool _inventoryFull;

        public CraftableProductsComposer(
            List<(string Name, string Key, string DisplayName, string IconUrl)> recipes,
            List<(string Key, string DisplayName, string IconUrl, int Count)> pool,
            int craftSeconds,
            bool inventoryFull)
            : base(ServerPacketHeader.CraftableProductsMessageComposer)
        {
            _recipes = recipes;
            _pool = pool;
            _craftSeconds = craftSeconds;
            _inventoryFull = inventoryFull;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(_recipes.Count);
            foreach (var (Name, Key, DisplayName, IconUrl) in _recipes) {
                packet.WriteString(Name);
                packet.WriteString(Key);
                packet.WriteString(DisplayName);
                packet.WriteString(IconUrl);
            }
            packet.WriteInteger(_pool.Count);
            foreach (var (Key, DisplayName, IconUrl, Count) in _pool) {
                packet.WriteString(Key);
                packet.WriteString(DisplayName);
                packet.WriteString(IconUrl);
                packet.WriteInteger(Count);
            }
            packet.WriteInteger(_craftSeconds);
            packet.WriteBoolean(_inventoryFull);
        }
    }
}