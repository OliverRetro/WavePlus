using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Rooms.AI.Pets;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets.Horse
{
    internal class ModifyWhoCanRideHorseEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
                return;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out Room room))
                return;

            int petId = packet.PopInt();

            if (!room.GetRoomUserManager().TryGetPet(petId, out RoomUser pet))
                return;

            pet.PetData.AnyoneCanRide = pet.PetData.AnyoneCanRide == 1 ? 0 : 1;

            uint petDbId = (uint)petId;
            int anyoneRide = pet.PetData.AnyoneCanRide;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.BotsPetdata.Where(p => p.Id == petDbId).ExecuteUpdate(s => s.SetProperty(p => p.AnyoneRide, anyoneRide));
            }

            room.SendPacket(new PetInformationComposer(pet.PetData));
        }
    }
}