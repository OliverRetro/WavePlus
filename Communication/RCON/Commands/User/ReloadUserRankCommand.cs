using System.Linq;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Rcon.Commands.User
{
    internal class ReloadUserRankCommand : IRconCommand
    {
        public string Description => "This command is used to reload a users rank and permissions.";

        public string Parameters => "%userId%";

        public bool TryExecute(string[] parameters)
        {
            if (!int.TryParse(parameters[0], out int userId))
                return false;

            GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
            if (client == null || client.GetHabbo() == null)
                return false;

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                client.GetHabbo().Rank = (int)(db.Users.Where(u => u.Id == userId).Select(u => u.Rank).FirstOrDefault() ?? 0);
            }

            client.GetHabbo().GetPermissions().Init(client.GetHabbo());

            if (client.GetHabbo().GetPermissions().HasRight("mod_tickets")) {
                client.SendPacket(new ModeratorInitComposer(
                    PlusEnvironment.GetGame().GetModerationManager().UserMessagePresets,
                    PlusEnvironment.GetGame().GetModerationManager().RoomMessagePresets,
                    PlusEnvironment.GetGame().GetModerationManager().GetTickets));
            }

            return true;
        }
    }
}