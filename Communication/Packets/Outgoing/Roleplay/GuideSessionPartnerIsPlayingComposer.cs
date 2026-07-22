namespace Plus.Communication.Packets.Outgoing.Roleplay
{
    public class GuideSessionPartnerIsPlayingComposer(bool isPlaying) : MessageComposer(ServerPacketHeader.GuideSessionPartnerIsPlayingMessageComposer)
    {
        private readonly bool IsPlaying = isPlaying;

        public override void Compose(ServerPacket packet)
        {
            packet.WriteBoolean(IsPlaying);
        }
    }
}