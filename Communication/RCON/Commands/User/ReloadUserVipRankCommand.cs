using System.Linq;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Rcon.Commands.User
{
    internal class ReloadUserVipRankCommand : IRconCommand
    {
        public string Description => "This command is used to reload a users VIP rank and permissions.";

        public string Parameters => "%userId%";

        public bool TryExecute(string[] parameters)
        {
            if (!int.TryParse(parameters[0], out int userId))
                return false;

            GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
            if (client == null || client.GetHabbo() == null)
                return false;

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                client.GetHabbo().VipRank = db.Users.Where(u => u.Id == userId).Select(u => u.RankVip).FirstOrDefault() ?? 0;
            }

            client.GetHabbo().GetPermissions().Init(client.GetHabbo());
            return true;
        }
    }
}