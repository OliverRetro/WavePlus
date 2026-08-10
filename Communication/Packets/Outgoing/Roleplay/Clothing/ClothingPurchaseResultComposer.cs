namespace Plus.Communication.Packets.Outgoing.Roleplay.Clothing
{
    internal class ClothingPurchaseResultComposer : MessageComposer
    {
        private readonly bool _success;
        private readonly string _code;
        private readonly int _setId;
        private readonly int _newStock;

        public ClothingPurchaseResultComposer(bool success, string code, int setId, int newStock)
            : base(ServerPacketHeader.ClothingPurchaseResultMessageComposer)
        {
            _success = success;
            _code = code ?? string.Empty;
            _setId = setId;
            _newStock = newStock;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteBoolean(_success);
            packet.WriteString(_code);
            packet.WriteInteger(_setId);
            packet.WriteInteger(_newStock);
        }
    }
}