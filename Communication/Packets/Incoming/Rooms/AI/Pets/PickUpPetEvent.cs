using System.Drawing;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Inventory.Pets;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.AI;

namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets
{
    internal class PickUpPetEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
                return;

            if (session.GetHabbo() == null || session.GetHabbo().GetInventoryComponent() == null)
                return;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out Room room))
                return;

            int petId = packet.PopInt();

            if (!room.GetRoomUserManager().TryGetPet(petId, out RoomUser pet)) {
                //Check kick rights, just because it seems most appropriate.
                if ((!room.CheckRights(session) && room.WhoCanKick != 2 && room.Group == null) || (room.Group != null && !room.CheckRights(session, false, true)))
                    return;

                //Okay so, we've established we have no pets in this room by this virtual Id, let us check out users, maybe they're creeping as a pet?!
                RoomUser targetUser = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(petId);

                //Check some values first, please!
                if (targetUser?.GetClient() == null || targetUser.GetClient().GetHabbo() == null)
                    return;

                //Update the targets PetId.
                targetUser.GetClient().GetHabbo().PetId = 0;

                //Quickly remove the old user instance.
                room.SendPacket(new UserRemoveComposer(targetUser.VirtualId));

                //Add the new one, they won't even notice a thing!!11 8-)
                room.SendPacket(new UsersComposer(targetUser));
                return;
            }

            if (session.GetHabbo().Id != pet.PetData.OwnerId && !room.CheckRights(session, true)) {
                session.SendWhisper("You can only pickup your own pets, to kick a pet you must have room rights.");
                return;
            }

            if (pet.RidingHorse) {
                RoomUser userRiding = room.GetRoomUserManager().GetRoomUserByVirtualId(pet.HorseId);
                if (userRiding != null) {
                    userRiding.RidingHorse = false;
                    userRiding.ApplyEffect(-1);
                    userRiding.MoveTo(new Point(userRiding.X + 1, userRiding.Y + 1));
                } else
                    pet.RidingHorse = false;
            }

            Pet data = pet.PetData;
            if (data != null) {
                uint petDbId = (uint)data.PetId;
                int experience = data.Experience;
                int energy = data.Energy;
                int nutrition = data.Nutrition;
                int respect = data.Respect;
                using WavePlusContext db = PlusEnvironment.GetDbContext();
                db.Bots.Where(b => b.Id == petDbId).ExecuteUpdate(s => s
                    .SetProperty(b => b.RoomId, 0u)
                    .SetProperty(b => b.X, 0)
                    .SetProperty(b => b.Y, 0)
                    .SetProperty(b => b.Z, 0));
                db.BotsPetdata.Where(p => p.Id == petDbId).ExecuteUpdate(s => s
                    .SetProperty(p => p.Experience, experience)
                    .SetProperty(p => p.Energy, energy)
                    .SetProperty(p => p.Nutrition, nutrition)
                    .SetProperty(p => p.Respect, respect));
            }

            if (data.OwnerId != session.GetHabbo().Id) {
                GameClient target = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(data.OwnerId);
                if (target != null) {
                    if (target.GetHabbo().GetInventoryComponent().TryAddPet(pet.PetData)) {
                        pet.PetData.RoomId = 0;
                        pet.PetData.PlacedInRoom = false;

                        room.GetRoomUserManager().RemoveBot(pet.VirtualId, false);

                        target.SendPacket(new PetInventoryComposer(target.GetHabbo().GetInventoryComponent().GetPets()));
                        return;
                    }
                }
            }

            room.GetRoomUserManager().RemoveBot(pet.VirtualId, false);
        }
    }
}