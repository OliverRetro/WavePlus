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
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var userRow = db.users.Where(u => u.id == userId)
                    .Select(u => new { u.id, u.username, u.online, u.mail, u.ip_last, u.look, u.account_created, u.last_online })
                    .FirstOrDefault();

                if (userRow == null) {
                    session.SendNotification(PlusEnvironment.GetLanguageManager().TryGetValue("user.not_found"));
                    return;
                }

                var infoRow = db.user_infos.Where(i => i.user_id == userId)
                    .Select(i => new { i.cfhs, i.cfhs_abusive, i.cautions, i.bans, i.trading_locked, i.trading_locks_count })
                    .FirstOrDefault();

                if (infoRow == null) {
                    db.Database.ExecuteSqlInterpolated($"INSERT INTO `user_info` (`user_id`) VALUES ({userId})");
                    infoRow = db.user_infos.Where(i => i.user_id == userId)
                        .Select(i => new { i.cfhs, i.cfhs_abusive, i.cautions, i.bans, i.trading_locked, i.trading_locks_count })
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
                user = userTable.Rows.Add(userRow.id, userRow.username, userRow.online, userRow.mail, userRow.ip_last,
                    userRow.look, userRow.account_created, userRow.last_online ?? 0);

                DataTable infoTable = new();
                infoTable.Columns.Add("cfhs", typeof(int));
                infoTable.Columns.Add("cfhs_abusive", typeof(int));
                infoTable.Columns.Add("cautions", typeof(int));
                infoTable.Columns.Add("bans", typeof(int));
                infoTable.Columns.Add("trading_locked", typeof(double));
                infoTable.Columns.Add("trading_locks_count", typeof(int));
                info = infoTable.Rows.Add(infoRow.cfhs, infoRow.cfhs_abusive, infoRow.cautions, infoRow.bans,
                    infoRow.trading_locked, infoRow.trading_locks_count);
            }

            session.SendPacket(new ModeratorUserInfoComposer(user, info));
        }
    }
}
