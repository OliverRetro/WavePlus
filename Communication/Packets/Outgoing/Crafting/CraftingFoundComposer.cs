namespace Plus.Communication.Packets.Outgoing.Crafting
{
    internal class CraftingFoundComposer : MessageComposer
    {
        private readonly int _state;
        private readonly int _count;
        private readonly string _recipeName;
        private readonly string _rewardKey;
        private readonly string _rewardName;
        private readonly string _rewardIcon;

        public CraftingFoundComposer(int state, int count, string recipeName = null, string rewardKey = null, string rewardName = null, string rewardIcon = null)
            : base(ServerPacketHeader.CraftingFoundMessageComposer)
        {
            _state = state;
            _count = count;
            _recipeName = recipeName ?? "";
            _rewardKey = rewardKey ?? "";
            _rewardName = rewardName ?? "";
            _rewardIcon = rewardIcon ?? "";
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(_state);
            packet.WriteInteger(_count);
            if (_state == 2) {
                packet.WriteString(_recipeName);
                packet.WriteString(_rewardKey);
                packet.WriteString(_rewardName);
                packet.WriteString(_rewardIcon);
            }
        }
    }
}