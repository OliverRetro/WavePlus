using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Moderation
{
    internal class ModerationBanEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().GetPermissions().HasRight("mod_soft_ban"))
                return;

            int userId = packet.PopInt();
            string message = packet.PopString();
            double length = packet.PopInt() * 3600 + PlusEnvironment.GetUnixTimestamp();
            packet.PopString(); //unk1
            packet.PopString(); //unk2
            bool ipBan = packet.PopBoolean();
            bool machineBan = packet.PopBoolean();

            if (machineBan)
                ipBan = false;

            Habbo habbo = PlusEnvironment.GetHabboById(userId);

            if (habbo == null) {
                session.SendWhisper("An error occoured whilst finding that user in the database.");
                return;
            }

            if (habbo.GetPermissions().HasRight("mod_tool") && !session.GetHabbo().GetPermissions().HasRight("mod_ban_any")) {
                session.SendWhisper("Oops, you cannot ban that user.");
                return;
            }

            message = message != null ? message : "No reason was given.";

            int targetId = habbo.Id;
            string ipLast;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.UserInfos.Where(u => u.UserId == targetId)
                    .ExecuteUpdate(s => s.SetProperty(u => u.Bans, u => u.Bans + 1));
                ipLast = db.Users.Where(u => u.Id == targetId).Select(u => u.IpLast).FirstOrDefault() ?? string.Empty;
            }

            if ((ipBan || machineBan) && !string.IsNullOrEmpty(ipLast))
                PlusEnvironment.GetGame().GetModerationManager().BanUser(session.GetHabbo().Username, ModerationBanType.Ip, ipLast, message, length, targetId);
            else
                PlusEnvironment.GetGame().GetModerationManager().BanUser(session.GetHabbo().Username, ModerationBanType.Username, habbo.Username, message, length, targetId);

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(habbo.Username);
            targetClient?.Disconnect(immediate: true);
        }
    }
}