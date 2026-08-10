using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class BanCommand : IChatCommand
    {
        public string PermissionRequired => "command_ban";

        public string Parameters => "%username% %hours% %reason%";

        public string Description => "Account ban a player from the hotel for a fixed number of hours.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 3) {
                session.SendWhisper("Usage: :ban <username> <hours> [reason]  (use 'perm' for a permanent ban)", 1);
                return;
            }

            Habbo habbo = PlusEnvironment.GetHabboByUsername(@params[1]);
            if (habbo == null) {
                session.SendWhisper("An error occoured whilst finding that user in the database.", 1);
                return;
            }

            if (habbo.GetPermissions().HasRight("mod_soft_ban") && !session.GetHabbo().GetPermissions().HasRight("mod_ban_any")) {
                session.SendWhisper("Oops, you cannot ban that user.");
                return;
            }

            string hours = @params[2];
            double expire;
            if (string.IsNullOrEmpty(hours) || hours.Equals("perm", StringComparison.OrdinalIgnoreCase)) {
                expire = PlusEnvironment.GetUnixTimestamp() + 78892200;
            } else if (double.TryParse(hours, out double parsedHours) && parsedHours > 0) {
                expire = PlusEnvironment.GetUnixTimestamp() + (parsedHours * 3600);
            } else {
                session.SendWhisper("'" + hours + "' is not a valid number of hours. Use a number or 'perm'.", 1);
                return;
            }

            string reason = @params.Length >= 4 ? CommandManager.MergeParams(@params, 3) : "No reason was given.";

            string username = habbo.Username;
            int targetId = habbo.Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                db.UserInfos.Where(u => u.UserId == targetId)
                    .ExecuteUpdate(s => s.SetProperty(u => u.Bans, u => u.Bans + 1));

            PlusEnvironment.GetGame().GetModerationManager().BanUser(session.GetHabbo().Username, ModerationBanType.Username, username, reason, expire, targetId);

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(username);
            targetClient?.Disconnect();

            session.SendWhisper("Success, you have account banned the user '" + username + "' for " + hours + " hour(s) with the reason '" + reason + "'!", 1);
        }
    }
}