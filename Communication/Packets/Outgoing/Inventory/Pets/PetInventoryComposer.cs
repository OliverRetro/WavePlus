using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.Rooms.AI;

namespace Plus.Communication.Packets.Outgoing.Inventory.Pets
{
    internal class PetInventoryComposer : MessageComposer
    {
        public ICollection<Pet> Pets { get; }

        public PetInventoryComposer(ICollection<Pet> pets)
            : base(ServerPacketHeader.PetInventoryMessageComposer)
        {
            Pets = pets;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(1);
            packet.WriteInteger(1);
            packet.WriteInteger(Pets.Count);
            foreach (Pet pet in Pets.ToList()) {
                packet.WriteInteger(pet.PetId);
                packet.WriteString(pet.Name);
                packet.WriteInteger(pet.Type);
                packet.WriteInteger(int.Parse(pet.Race));
                packet.WriteString(pet.Color);
                packet.WriteInteger(0);
                packet.WriteInteger(0);
                packet.WriteInteger(0);
            }
        }
    }
}