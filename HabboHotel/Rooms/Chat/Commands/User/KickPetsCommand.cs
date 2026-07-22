using System.Drawing;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Inventory.Pets;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.AI;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class KickPetsCommand : IChatCommand
    {
        public string PermissionRequired => "command_kickpets";

        public string Parameters => "";

        public string Description => "Kick all of the pets from the room.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (!room.CheckRights(session, true)) {
                session.SendWhisper("Oops, only the room owner can run this command!");
                return;
            }

            if (room.GetRoomUserManager().GetPets().Count == 0) {
                session.SendWhisper("Oops, there isn't any pets in here!?");
            }

            foreach (RoomUser bot in room.GetRoomUserManager().GetUserList().ToList()) {
                if (bot == null)
                    continue;

                if (bot.RidingHorse) {
                    RoomUser rider = room.GetRoomUserManager().GetRoomUserByVirtualId(bot.HorseId);
                    if (rider != null) {
                        rider.RidingHorse = false;
                        rider.ApplyEffect(-1);
                        rider.MoveTo(new Point(rider.X + 1, rider.Y + 1));
                    } else
                        bot.RidingHorse = false;
                }

                Pet pet = bot.PetData;
                if (pet != null) {
                    return;
                }

                pet.RoomId = 0;
                pet.PlacedInRoom = false;

                room.GetRoomUserManager().RemoveBot(bot.VirtualId, false);

                if (pet.OwnerId != session.GetHabbo().Id) {
                    GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(pet.OwnerId);
                    if (targetClient != null) {
                        if (targetClient.GetHabbo().GetInventoryComponent().TryAddPet(pet)) {
                            targetClient.SendPacket(new PetInventoryComposer(targetClient.GetHabbo().GetInventoryComponent().GetPets()));
                        }
                    }
                }

                if (session.GetHabbo().GetInventoryComponent().TryAddPet(pet)) {
                    session.SendPacket(new PetInventoryComposer(session.GetHabbo().GetInventoryComponent().GetPets()));
                }

                uint petId = (uint)pet.PetId;
                int petExperience = pet.Experience;
                int petEnergy = pet.Energy;
                int petNutrition = pet.Nutrition;
                int petRespect = pet.Respect;
                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    db.Bots.Where(b => b.Id == petId).ExecuteUpdate(s => s.SetProperty(b => b.RoomId, 0u).SetProperty(b => b.X, 0).SetProperty(b => b.Y, 0).SetProperty(b => b.Z, 0));
                    db.BotsPetdata.Where(p => p.Id == petId).ExecuteUpdate(s => s.SetProperty(p => p.Experience, petExperience).SetProperty(p => p.Energy, petEnergy).SetProperty(p => p.Nutrition, petNutrition).SetProperty(p => p.Respect, petRespect));
                }
            }

            session.SendWhisper("All pets have been kicked from the room.");
        }
    }
}