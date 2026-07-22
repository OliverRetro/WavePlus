using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.Catalog.Pets;

namespace Plus.Communication.Packets.Outgoing.Catalog
{
    public class SellablePetBreedsComposer : MessageComposer
    {
        public string PetType { get; }
        public int PetId { get; }
        public ICollection<PetRace> Races { get; }

        public SellablePetBreedsComposer(string petType, int petId, ICollection<PetRace> races)
            : base(ServerPacketHeader.SellablePetBreedsMessageComposer)
        {
            PetType = petType;
            PetId = petId;
            Races = races;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteString(PetType);

            packet.WriteInteger(Races.Count);
            foreach (PetRace race in Races.ToList()) {
                packet.WriteInteger(PetId);
                packet.WriteInteger(race.PrimaryColour);
                packet.WriteInteger(race.SecondaryColour);
                packet.WriteBoolean(race.HasPrimaryColour);
                packet.WriteBoolean(race.HasSecondaryColour);
            }
        }
    }
}