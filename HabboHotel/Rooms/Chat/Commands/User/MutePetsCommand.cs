using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class MutePetsCommand : IChatCommand
    {
        public string PermissionRequired => "command_mute_pets";

        public string Parameters => "";

        public string Description => "Ignore pet chat or enable it again.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            session.GetHabbo().AllowPetSpeech = !session.GetHabbo().AllowPetSpeech;

            int userId = session.GetHabbo().Id;
            string petsMuted = session.GetHabbo().AllowPetSpeech ? "1" : "0";
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.Users.Where(u => u.Id == userId).ExecuteUpdate(s => s.SetProperty(u => u.PetsMuted, petsMuted));
            }

            if (session.GetHabbo().AllowPetSpeech)
                session.SendWhisper("Change successful, you can no longer see speech from pets.");
            else
                session.SendWhisper("Change successful, you can now see speech from pets.");
        }
    }
}