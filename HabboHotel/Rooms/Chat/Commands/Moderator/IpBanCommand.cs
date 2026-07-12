using System;
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

        public string Parameters => "%username%";

        public string Description => "IP and account ban another user.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1) {
                session.SendWhisper("Please enter the username of the user you'd like to IP ban & account ban.");
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
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                db.user_infos.Where(u => u.user_id == habboId).ExecuteUpdate(s => s.SetProperty(u => u.bans, u => u.bans + 1));
                ipAddress = db.users.Where(u => u.id == habboId).Select(u => u.ip_last).FirstOrDefault() ?? string.Empty;
            }

            string reason;
            if (@params.Length >= 3)
                reason = CommandManager.MergeParams(@params, 2);
            else
                reason = "No reason specified.";

            if (!string.IsNullOrEmpty(ipAddress))
                PlusEnvironment.GetGame().GetModerationManager().BanUser(session.GetHabbo().Username, ModerationBanType.Ip, ipAddress, reason, expire);
            PlusEnvironment.GetGame().GetModerationManager().BanUser(session.GetHabbo().Username, ModerationBanType.Username, habbo.Username, reason, expire);

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(username);
            targetClient?.Disconnect();


            session.SendWhisper("Success, you have IP and account banned the user '" + username + "' for '" + reason + "'!");
        }
    }
}