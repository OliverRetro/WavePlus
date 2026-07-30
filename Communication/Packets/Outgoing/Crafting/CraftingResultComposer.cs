namespace Plus.Communication.Packets.Outgoing.Crafting
{
    internal class CraftingResultComposer : MessageComposer
    {
        private readonly bool _success;
        private readonly string _recipeName;
        private readonly string _rewardKey;
        private readonly string _displayName;
        private readonly string _iconUrl;

        public CraftingResultComposer(bool success, string recipeName = null, string rewardKey = null, string displayName = null, string iconUrl = null)
            : base(ServerPacketHeader.CraftingResultMessageComposer)
        {
            _success = success;
            _recipeName = recipeName ?? "";
            _rewardKey = rewardKey ?? "";
            _displayName = displayName ?? "";
            _iconUrl = iconUrl ?? "";
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteBoolean(_success);
            if (_success) {
                packet.WriteString(_recipeName);
                packet.WriteString(_rewardKey);
                packet.WriteString(_displayName);
                packet.WriteString(_iconUrl);
            }
        }
    }
}