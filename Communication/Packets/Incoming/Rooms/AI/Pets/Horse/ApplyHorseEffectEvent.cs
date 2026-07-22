using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Rooms.AI.Pets;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets.Horse
{
    internal class ApplyHorseEffectEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
                return;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out Room room))
                return;

            int itemId = packet.PopInt();
            Item item = room.GetRoomItemHandler().GetItem(itemId);
            if (item == null)
                return;

            int petId = packet.PopInt();

            if (!room.GetRoomUserManager().TryGetPet(petId, out RoomUser petUser))
                return;

            if (petUser.PetData == null || petUser.PetData.OwnerId != session.GetHabbo().Id)
                return;

            if (item.Data.InteractionType == InteractionType.HorseSaddle1) {
                petUser.PetData.Saddle = 9;
                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    uint petDataId = (uint)petUser.PetData.PetId;
                    uint itmId = (uint)item.Id;
                    db.BotsPetdata.Where(p => p.Id == petDataId).ExecuteUpdate(s => s.SetProperty(p => p.HaveSaddle, 9));
                    db.Items.Where(i => i.Id == itmId).ExecuteDelete();
                }

                //We only want to use this if we're successful. 
                room.GetRoomItemHandler().RemoveFurniture(session, item.Id);
            } else if (item.Data.InteractionType == InteractionType.HorseSaddle2) {
                petUser.PetData.Saddle = 10;
                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    uint petDataId = (uint)petUser.PetData.PetId;
                    uint itmId = (uint)item.Id;
                    db.BotsPetdata.Where(p => p.Id == petDataId).ExecuteUpdate(s => s.SetProperty(p => p.HaveSaddle, 10));
                    db.Items.Where(i => i.Id == itmId).ExecuteDelete();
                }

                //We only want to use this if we're successful. 
                room.GetRoomItemHandler().RemoveFurniture(session, item.Id);
            } else if (item.Data.InteractionType == InteractionType.HorseHairstyle) {
                int parse = 100;
                string hairType = item.GetBaseItem().ItemName.Split('_')[2];

                parse += int.Parse(hairType);

                petUser.PetData.PetHair = parse;
                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    uint petDataId = (uint)petUser.PetData.PetId;
                    uint itmId = (uint)item.Id;
                    int petHair = petUser.PetData.PetHair;
                    db.BotsPetdata.Where(p => p.Id == petDataId).ExecuteUpdate(s => s.SetProperty(p => p.Pethair, petHair));
                    db.Items.Where(i => i.Id == itmId).ExecuteDelete();
                }

                //We only want to use this if we're successful. 
                room.GetRoomItemHandler().RemoveFurniture(session, item.Id);
            } else if (item.Data.InteractionType == InteractionType.HorseHairDye) {
                int hairDye = 48;
                string hairType = item.GetBaseItem().ItemName.Split('_')[2];

                hairDye += int.Parse(hairType);
                petUser.PetData.HairDye = hairDye;

                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    uint petDataId = (uint)petUser.PetData.PetId;
                    uint itmId = (uint)item.Id;
                    int dye = petUser.PetData.HairDye;
                    db.BotsPetdata.Where(p => p.Id == petDataId).ExecuteUpdate(s => s.SetProperty(p => p.Hairdye, dye));
                    db.Items.Where(i => i.Id == itmId).ExecuteDelete();
                }

                //We only want to use this if we're successful. 
                room.GetRoomItemHandler().RemoveFurniture(session, item.Id);
            } else if (item.Data.InteractionType == InteractionType.HorseBodyDye) {
                string race = item.GetBaseItem().ItemName.Split('_')[2];
                int parse = int.Parse(race);
                int raceLast = 2 + (parse * 4) - 4;
                if (parse == 13)
                    raceLast = 61;
                else if (parse == 14)
                    raceLast = 65;
                else if (parse == 15)
                    raceLast = 69;
                else if (parse == 16)
                    raceLast = 73;
                petUser.PetData.Race = raceLast.ToString();

                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    uint petDataId = (uint)petUser.PetData.PetId;
                    uint itmId = (uint)item.Id;
                    string race2 = petUser.PetData.Race;
                    db.BotsPetdata.Where(p => p.Id == petDataId).ExecuteUpdate(s => s.SetProperty(p => p.Race, race2));
                    db.Items.Where(i => i.Id == itmId).ExecuteDelete();
                }

                //We only want to use this if we're successful. 
                room.GetRoomItemHandler().RemoveFurniture(session, item.Id);
            }

            //Update the Pet and the Pet figure information.
            room.SendPacket(new UsersComposer(petUser));
            room.SendPacket(new PetHorseFigureInformationComposer(petUser));
        }
    }
}