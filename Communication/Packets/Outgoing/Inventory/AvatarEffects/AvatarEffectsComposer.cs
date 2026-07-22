using System.Collections.Generic;
using Plus.HabboHotel.Users.Effects;

namespace Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects
{
    internal class AvatarEffectsComposer : MessageComposer
    {
        public ICollection<AvatarEffect> Effects { get; }

        public AvatarEffectsComposer(ICollection<AvatarEffect> effects)
            : base(ServerPacketHeader.AvatarEffectsMessageComposer)
        {
            Effects = effects;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Effects.Count);

            foreach (AvatarEffect effect in Effects) {
                packet.WriteInteger(effect.SpriteId); //Effect Id
                packet.WriteInteger(0); //Type, 0 = Hand, 1 = Full
                packet.WriteInteger((int)effect.Duration);
                packet.WriteInteger(effect.Activated ? effect.Quantity - 1 : effect.Quantity);
                packet.WriteInteger(effect.Activated ? (int)effect.TimeLeft : -1);
                packet.WriteBoolean(false); //Permanent
            }
        }
    }
}