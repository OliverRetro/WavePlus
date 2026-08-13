using System;

namespace Plus.Communication.Packets.Outgoing.Roleplay
{
    /// <summary>
    /// Tells the client how fast one avatar walks, so it slides and animates a tile over the same
    /// time the server paces steps at (500ms / speed). Without it a fast tier looks like the avatar
    /// covers part of a tile and then jumps the rest.
    /// </summary>
    internal class AvatarWalkSpeedComposer : MessageComposer
    {
        private readonly int _virtualId;
        private readonly double _walkSpeed;
        private readonly int _animationMs;

        public AvatarWalkSpeedComposer(int virtualId, double walkSpeed, int animationMs)
            : base(ServerPacketHeader.AvatarWalkSpeedMessageComposer)
        {
            _virtualId = virtualId;
            _walkSpeed = walkSpeed;
            _animationMs = animationMs;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(_virtualId);

            // Sent as hundredths so the tiers survive the integer wire format: 100 = 1.0, 250 = 2.5.
            // Drives how fast the legs cycle.
            packet.WriteInteger((int)Math.Round(_walkSpeed * 100));

            // How long to slide across a tile. Sent separately from the speed because it carries a
            // little slack over the server's own step interval — see RoomUser.StepAnimationMs.
            packet.WriteInteger(_animationMs);
        }
    }
}