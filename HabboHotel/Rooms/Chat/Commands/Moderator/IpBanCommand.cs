using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class IpBanCommand : IChatCommand
    {
        public string PermissionRequired => "command_ip_ban";

        public string Parameters => "%username% %reason%";

        public string Description => "IP ban a user by their last-connected address.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1) {
                session.SendWhisper("Please enter the username of the user you'd like to IP ban.");
                return;
            }

            Habbo habbo = PlusEnvironment.GetHabboByUsername(@params[1]);
            if (habbo == null) {
                session.SendWhisper("An error occoured whilst finding that user in the database.");
                return;
            }

            if (habbo.GetPermissions().HasRight("mod_tool") && !session.GetHabbo().GetPermissions().HasRight("mod_ban_any")) {
                session.SendWhisper("Oops, you cannot ban that user.");
                return;
            }

            string ipAddress = string.Empty;
            double expire = PlusEnvironment.GetUnixTimestamp() + 78892200;
            string username = habbo.Username;
            int habboId = habbo.Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.UserInfos.Where(u => u.UserId == habboId).ExecuteUpdate(s => s.SetProperty(u => u.Bans, u => u.Bans + 1));
                ipAddress = db.Users.Where(u => u.Id == habboId).Select(u => u.IpLast).FirstOrDefault() ?? string.Empty;
            }

            if (string.IsNullOrEmpty(ipAddress)) {
                session.SendWhisper("That user has no recorded IP address to ban.");
                return;
            }

            string reason = @params.Length >= 3 ? CommandManager.MergeParams(@params, 2) : "No reason was given.";

            PlusEnvironment.GetGame().GetModerationManager().BanUser(session.GetHabbo().Username, ModerationBanType.Ip, ipAddress, reason, expire, habboId);

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(username);
            targetClient?.Disconnect();

            session.SendWhisper("Success, you have IP banned the user '" + username + "' (" + ipAddress + ") with the reason '" + reason + "'!");
        }
    }
}