using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class DisableMimicCommand : IChatCommand
    {
        public string PermissionRequired => "command_disable_mimic";

        public string Parameters => "";

        public string Description => "Allows you to disable the ability to be mimiced or to enable the ability to be mimiced.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            session.GetHabbo().AllowMimic = !session.GetHabbo().AllowMimic;
            session.SendWhisper("You're " + (session.GetHabbo().AllowMimic ? "now" : "no longer") + " able to be mimiced.");

            int userId = session.GetHabbo().Id;
            string allowMimic = PlusEnvironment.BoolToEnum(session.GetHabbo().AllowMimic);

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            db.Users.Where(u => u.Id == userId).ExecuteUpdate(s => s.SetProperty(u => u.AllowMimic, allowMimic));
        }
    }
}