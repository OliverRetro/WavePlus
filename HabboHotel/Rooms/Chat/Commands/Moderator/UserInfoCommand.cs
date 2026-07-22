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

            var userData = db.Users
                .Where(u => u.Username == username)
                .Select(u => new { u.Id, u.Username, u.Rank, u.RankVip, u.Credits, u.ActivityPoints, u.VipPoints, u.GotwPoints })
                .FirstOrDefault();

            if (userData == null) {
                session.SendNotification("Oops, there is no user in the database with that username (" + username + ")!");
                return;
            }

            int userId = userData.Id;

            var userInfo = db.UserInfos
                .Where(ui => ui.UserId == userId)
                .Select(ui => new { ui.Bans, ui.Cfhs, ui.CfhsAbusive, ui.TradingLocked, ui.TradingLocksCount })
                .FirstOrDefault();

            if (userInfo == null) {
                db.Database.ExecuteSqlInterpolated($"INSERT INTO `user_info` (`user_id`) VALUES ({userId})");

                userInfo = db.UserInfos
                    .Where(ui => ui.UserId == userId)
                    .Select(ui => new { ui.Bans, ui.Cfhs, ui.CfhsAbusive, ui.TradingLocked, ui.TradingLocksCount })
                    .FirstOrDefault();
            }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(username);

            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(userInfo.TradingLocked);

            StringBuilder habboInfo = new();
            habboInfo.Append("<b>Generic Info:</b>\r");
            habboInfo.Append("Username: " + userData.Username + "" + " (ID: <b>" + userData.Id + ")</b>\r\r");
            habboInfo.Append("Rank: " + (userData.Rank ?? 0) + "\r");
            habboInfo.Append("VIP Rank: " + (userData.RankVip ?? 0) + "\r");
            habboInfo.Append("Online Status: " + (targetClient != null ? "True" : "False") + "\r\r");

            habboInfo.Append("<b>Currency Info:</b>\r");
            habboInfo.Append("Credits: " + (userData.Credits ?? 0) + "\r");
            habboInfo.Append("Duckets: " + (userData.ActivityPoints ?? 0) + "\r");
            habboInfo.Append("Diamonds: " + (userData.VipPoints ?? 0) + "\r");
            habboInfo.Append("GOTW Points: " + (userData.GotwPoints ?? 0) + "\r\r");

            habboInfo.Append("<b>Moderation Info:</b>\r");
            habboInfo.Append("Bans: " + userInfo.Bans + "\r");
            habboInfo.Append("CFHs Sent: " + userInfo.Cfhs + "\r");
            habboInfo.Append("Abusive CFHs: " + userInfo.CfhsAbusive + "\r");
            habboInfo.Append("Trading Locked: " + (Convert.ToInt32(userInfo.TradingLocked) == 0 ? "No outstanding lock" : "Expiry: " + (origin.ToString("dd/MM/yyyy")) + "") + "\r");
            habboInfo.Append("Amount of trading locks: " + userInfo.TradingLocksCount + "\r\r");

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