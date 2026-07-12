using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class MuteBotsCommand : IChatCommand
    {
        public string PermissionRequired => "command_mute_bots";

        public string Parameters => "";

        public string Description => "Ignore bot chat or enable it again.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            session.GetHabbo().AllowBotSpeech = !session.GetHabbo().AllowBotSpeech;

            int userId = session.GetHabbo().Id;
            string botsMuted = session.GetHabbo().AllowBotSpeech ? "1" : "0";
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                db.users.Where(u => u.id == userId).ExecuteUpdate(s => s.SetProperty(u => u.bots_muted, botsMuted));
            }

            if (session.GetHabbo().AllowBotSpeech)
                session.SendWhisper("Change successful, you can no longer see speech from bots.");
            else
                session.SendWhisper("Change successful, you can now see speech from bots.");
        }
    }
}
