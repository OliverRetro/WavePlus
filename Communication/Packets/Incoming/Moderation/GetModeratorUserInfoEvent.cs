using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Moderation
{
    internal class GetModeratorUserInfoEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                return;

            int userId = packet.PopInt();

            DataRow user;
            DataRow info;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var userRow = db.Users.Where(u => u.Id == userId)
                    .Select(u => new { u.Id, u.Username, u.Online, u.Mail, u.IpLast, u.Look, u.AccountCreated, u.LastOnline })
                    .FirstOrDefault();

                if (userRow == null) {
                    session.SendNotification(PlusEnvironment.GetLanguageManager().TryGetValue("user.not_found"));
                    return;
                }

                var infoRow = db.UserInfos.Where(i => i.UserId == userId)
                    .Select(i => new { i.Cfhs, i.CfhsAbusive, i.Cautions, i.Bans, i.TradingLocked, i.TradingLocksCount })
                    .FirstOrDefault();

                if (infoRow == null) {
                    db.Database.ExecuteSqlInterpolated($"INSERT INTO `user_info` (`user_id`) VALUES ({userId})");
                    infoRow = db.UserInfos.Where(i => i.UserId == userId)
                        .Select(i => new { i.Cfhs, i.CfhsAbusive, i.Cautions, i.Bans, i.TradingLocked, i.TradingLocksCount })
                        .FirstOrDefault();
                }

                DataTable userTable = new();
                userTable.Columns.Add("id", typeof(int));
                userTable.Columns.Add("username", typeof(string));
                userTable.Columns.Add("online", typeof(string));
                userTable.Columns.Add("mail", typeof(string));
                userTable.Columns.Add("ip_last", typeof(string));
                userTable.Columns.Add("look", typeof(string));
                userTable.Columns.Add("account_created", typeof(string));
                userTable.Columns.Add("last_online", typeof(int));
                user = userTable.Rows.Add(userRow.Id, userRow.Username, userRow.Online, userRow.Mail, userRow.IpLast,
                    userRow.Look, userRow.AccountCreated, userRow.LastOnline ?? 0);

                DataTable infoTable = new();
                infoTable.Columns.Add("cfhs", typeof(int));
                infoTable.Columns.Add("cfhs_abusive", typeof(int));
                infoTable.Columns.Add("cautions", typeof(int));
                infoTable.Columns.Add("bans", typeof(int));
                infoTable.Columns.Add("trading_locked", typeof(double));
                infoTable.Columns.Add("trading_locks_count", typeof(int));
                info = infoTable.Rows.Add(infoRow.Cfhs, infoRow.CfhsAbusive, infoRow.Cautions, infoRow.Bans,
                    infoRow.TradingLocked, infoRow.TradingLocksCount);
            }

            session.SendPacket(new ModeratorUserInfoComposer(user, info));
        }
    }
}