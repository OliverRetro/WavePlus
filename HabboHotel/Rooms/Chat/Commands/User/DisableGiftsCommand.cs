using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class DisableGiftsCommand : IChatCommand
    {
        public string PermissionRequired => "command_disable_gifts";

        public string Parameters => "";

        public string Description => "Allows you to disable the ability to receive gifts or to enable the ability to receive gifts.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            session.GetHabbo().AllowGifts = !session.GetHabbo().AllowGifts;
            session.SendWhisper("You're " + (session.GetHabbo().AllowGifts ? "now" : "no longer") + " accepting gifts.");

            int userId = session.GetHabbo().Id;
            string allowGifts = PlusEnvironment.BoolToEnum(session.GetHabbo().AllowGifts);

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            db.Users.Where(u => u.Id == userId).ExecuteUpdate(s => s.SetProperty(u => u.AllowGifts, allowGifts));
        }
    }
}