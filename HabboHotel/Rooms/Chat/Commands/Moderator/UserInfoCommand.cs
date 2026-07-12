using System;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class UserInfoCommand : IChatCommand
    {
        public string PermissionRequired => "command_user_info";

        public string Parameters => "%username%";

        public string Description => "View another users profile information.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1) {
                session.SendWhisper("Please enter the username of the user you wish to view.");
                return;
            }

            string username = @params[1];

            using WavePlusContext db = PlusEnvironment.GetDbContext();

            var userData = db.users
                .Where(u => u.username == username)
                .Select(u => new { u.id, u.username, u.rank, u.rank_vip, u.credits, u.activity_points, u.vip_points, u.gotw_points })
                .FirstOrDefault();

            if (userData == null) {
                session.SendNotification("Oops, there is no user in the database with that username (" + username + ")!");
                return;
            }

            int userId = userData.id;

            var userInfo = db.user_infos
                .Where(ui => ui.user_id == userId)
                .Select(ui => new { ui.bans, ui.cfhs, ui.cfhs_abusive, ui.trading_locked, ui.trading_locks_count })
                .FirstOrDefault();

            if (userInfo == null) {
                db.Database.ExecuteSqlInterpolated($"INSERT INTO `user_info` (`user_id`) VALUES ({userId})");

                userInfo = db.user_infos
                    .Where(ui => ui.user_id == userId)
                    .Select(ui => new { ui.bans, ui.cfhs, ui.cfhs_abusive, ui.trading_locked, ui.trading_locks_count })
                    .FirstOrDefault();
            }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(username);

            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(userInfo.trading_locked);

            StringBuilder habboInfo = new();
            habboInfo.Append("<b>Generic Info:</b>\r");
            habboInfo.Append("Username: " + userData.username + "" + " (ID: <b>" + userData.id + ")</b>\r\r");
            habboInfo.Append("Rank: " + (userData.rank ?? 0) + "\r");
            habboInfo.Append("VIP Rank: " + (userData.rank_vip ?? 0) + "\r");
            habboInfo.Append("Online Status: " + (targetClient != null ? "True" : "False") + "\r\r");

            habboInfo.Append("<b>Currency Info:</b>\r");
            habboInfo.Append("Credits: " + (userData.credits ?? 0) + "\r");
            habboInfo.Append("Duckets: " + (userData.activity_points ?? 0) + "\r");
            habboInfo.Append("Diamonds: " + (userData.vip_points ?? 0) + "\r");
            habboInfo.Append("GOTW Points: " + (userData.gotw_points ?? 0) + "\r\r");

            habboInfo.Append("<b>Moderation Info:</b>\r");
            habboInfo.Append("Bans: " + userInfo.bans + "\r");
            habboInfo.Append("CFHs Sent: " + userInfo.cfhs + "\r");
            habboInfo.Append("Abusive CFHs: " + userInfo.cfhs_abusive + "\r");
            habboInfo.Append("Trading Locked: " + (Convert.ToInt32(userInfo.trading_locked) == 0 ? "No outstanding lock" : "Expiry: " + (origin.ToString("dd/MM/yyyy")) + "") + "\r");
            habboInfo.Append("Amount of trading locks: " + userInfo.trading_locks_count + "\r\r");

            if (targetClient != null) {
                habboInfo.Append("<b>Current Session:</b>\r");
                if (!targetClient.GetHabbo().InRoom)
                    habboInfo.Append("Currently not in a room.\r");
                else {
                    habboInfo.Append("Room: " + targetClient.GetHabbo().CurrentRoom.Name + " (" + targetClient.GetHabbo().CurrentRoom.RoomId + ")\r");
                    habboInfo.Append("Room Owner: " + targetClient.GetHabbo().CurrentRoom.OwnerName + "\r");
                    habboInfo.Append("Current Visitors: " + targetClient.GetHabbo().CurrentRoom.UsersNow + "/" + targetClient.GetHabbo().CurrentRoom.UsersMax);
                }
            }

            session.SendNotification(habboInfo.ToString());
        }
    }
}
