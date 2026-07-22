using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.Users.Clothing.Parts;

namespace Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects
{
    internal class FigureSetIdsComposer : MessageComposer
    {
        public ICollection<ClothingParts> ClothingParts { get; }

        public FigureSetIdsComposer(ICollection<ClothingParts> clothingParts)
            : base(ServerPacketHeader.FigureSetIdsMessageComposer)
        {
            ClothingParts = clothingParts;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(ClothingParts.Count);
            foreach (ClothingParts part in ClothingParts.ToList()) {
                packet.WriteInteger(part.PartId);
            }

            packet.WriteInteger(ClothingParts.Count);
            foreach (ClothingParts part in ClothingParts.ToList()) {
                packet.WriteString(part.Part);
            }
        }
    }
}