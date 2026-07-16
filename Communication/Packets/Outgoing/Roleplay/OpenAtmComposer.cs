namespace Plus.Communication.Packets.Outgoing.Roleplay
{
    /// <summary>
    /// Tells the Nitro client to open the ATM view (header 6001). Sent by the server-side
    /// ATM furni interactor once the user is confirmed in range, so adjacency is enforced
    /// server-side. The balance itself is delivered via purse currency 200, so this only
    /// carries the withdrawal fee percentage for display.
    /// </summary>
    public class OpenAtmComposer : MessageComposer
    {
        private readonly int _withdrawFeePercent;
        private readonly int _depositFeePercent;

        public OpenAtmComposer(int withdrawFeePercent, int depositFeePercent)
            : base(ServerPacketHeader.OpenAtmMessageComposer)
        {
            _withdrawFeePercent = withdrawFeePercent;
            _depositFeePercent = depositFeePercent;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(_withdrawFeePercent);
            packet.WriteInteger(_depositFeePercent);
        }
    }
}
