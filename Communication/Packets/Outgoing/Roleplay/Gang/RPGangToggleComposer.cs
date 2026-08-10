namespace Plus.Communication.Packets.Outgoing.Roleplay.Gang
{
    // Tells the client to toggle the gang window open/closed. Fired when the gaz-js
    // Organization button is clicked, mirroring the :gang chat command.
    internal class RPGangToggleComposer : MessageComposer
    {
        public RPGangToggleComposer()
            : base(ServerPacketHeader.GangToggleMessageComposer)
        {
        }

        public override void Compose(ServerPacket packet)
        {
        }
    }
}