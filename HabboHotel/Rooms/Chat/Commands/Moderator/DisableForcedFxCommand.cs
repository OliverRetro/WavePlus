using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class DisableForcedFxCommand : IChatCommand
    {
        public string PermissionRequired => "command_forced_effects";

        public string Parameters => "";

        public string Description => "Gives you the ability to ignore or allow forced effects.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            session.GetHabbo().DisableForcedEffects = !session.GetHabbo().DisableForcedEffects;
            int userId = session.GetHabbo().Id;
            string disabled = (session.GetHabbo().DisableForcedEffects ? 1 : 0).ToString();
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                db.Users.Where(u => u.Id == userId)
                    .ExecuteUpdate(s => s.SetProperty(u => u.DisableForcedEffects, disabled));

            session.SendWhisper("Forced FX mode is now " + (session.GetHabbo().DisableForcedEffects ? "disabled!" : "enabled!"));
        }
    }
}