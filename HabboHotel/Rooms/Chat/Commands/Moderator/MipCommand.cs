using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class MipCommand : IChatCommand
    {
        // Ten years, in seconds. A mip is always at least this long.
        private const double TenYears = 315360000;

        public string PermissionRequired => "command_mip";

        public string Parameters => "%username% %reason%";

        public string Description => "IP ban (10 years) a user by their last-connected address.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1) {
                session.SendWhisper("Please enter the username of the user you'd like to IP ban.", 1);
                return;
            }

            Habbo habbo = PlusEnvironment.GetHabboByUsername(@params[1]);
            if (habbo == null) {
                session.SendWhisper("An error occoured whilst finding that user in the database.", 1);
                return;
            }

            if (habbo.GetPermissions().HasRight("mod_tool") && !session.GetHabbo().GetPermissions().HasRight("mod_ban_any")) {
                session.SendWhisper("Oops, you cannot ban that user.", 1);
                return;
            }

            string username = habbo.Username;
            int habboId = habbo.Id;

            string ipLast;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.UserInfos.Where(ui => ui.UserId == habboId).ExecuteUpdate(s => s.SetProperty(ui => ui.Bans, ui => ui.Bans + 1));
                ipLast = db.Users.Where(u => u.Id == habboId).Select(u => u.IpLast).FirstOrDefault() ?? string.Empty;
            }

            if (string.IsNullOrEmpty(ipLast)) {
                session.SendWhisper("That user has no recorded IP address to ban.", 1);
                return;
            }

            string reason = @params.Length >= 3 ? CommandManager.MergeParams(@params, 2) : "No reason was given.";
            double expire = PlusEnvironment.GetUnixTimestamp() + TenYears;

            PlusEnvironment.GetGame().GetModerationManager().BanUser(session.GetHabbo().Username, ModerationBanType.Ip, ipLast, reason, expire, habboId);

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(username);
            targetClient?.Disconnect();

            session.SendWhisper("Success, you have IP banned the user '" + username + " for 10 years with the reason '" + reason + "'!", 1);
        }
    }
}