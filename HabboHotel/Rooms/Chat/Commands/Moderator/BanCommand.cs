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

        public string Parameters => "%username% %length% %reason% ";

        public string Description => "Remove a toxic player from the hotel for a fixed amount of time.";

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

            if (habbo.GetPermissions().HasRight("mod_soft_ban") && !session.GetHabbo().GetPermissions().HasRight("mod_ban_any")) {
                session.SendWhisper("Oops, you cannot ban that user.");
                return;
            }

            double expire;
            string hours = @params[2];
            if (string.IsNullOrEmpty(hours) || hours == "perm")
                expire = PlusEnvironment.GetUnixTimestamp() + 78892200;
            else
                expire = (PlusEnvironment.GetUnixTimestamp() + (Convert.ToDouble(hours) * 3600));

            string reason;
            if (@params.Length >= 4)
                reason = CommandManager.MergeParams(@params, 3);
            else
                reason = "No reason specified.";

            string username = habbo.Username;
            int targetId = habbo.Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                db.user_infos.Where(u => u.user_id == targetId)
                    .ExecuteUpdate(s => s.SetProperty(u => u.bans, u => u.bans + 1));

            PlusEnvironment.GetGame().GetModerationManager().BanUser(session.GetHabbo().Username, ModerationBanType.Username, habbo.Username, reason, expire);

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(username);
            targetClient?.Disconnect();

            session.SendWhisper("Success, you have account banned the user '" + username + "' for " + hours + " hour(s) with the reason '" + reason + "'!");
        }
    }
}