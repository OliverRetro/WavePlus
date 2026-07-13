using Plus.HabboHotel.Users.Effects;

namespace Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects
{
    internal class AvatarEffectActivatedComposer : MessageComposer
    {
        public AvatarEffect Effect { get; }

        public AvatarEffectActivatedComposer(AvatarEffect effect)
            : base(ServerPacketHeader.AvatarEffectActivatedMessageComposer)
        {
            Effect = effect;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Effect.SpriteId);
            packet.WriteInteger((int)Effect.Duration);
            packet.WriteBoolean(false); //Permanent
        }
    }
}