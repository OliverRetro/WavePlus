using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Moderation
{
    internal class ModerationTradeLockEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().GetPermissions().HasRight("mod_trade_lock"))
                return;

            int userId = packet.PopInt();
            string message = packet.PopString();
            double days = packet.PopInt() / 1440.0;
            packet.PopString(); //unk1
            packet.PopString(); //unk2

            double length = PlusEnvironment.GetUnixTimestamp() + days * 86400;

            Habbo habbo = PlusEnvironment.GetHabboById(userId);
            if (habbo == null) {
                session.SendWhisper("An error occoured whilst finding that user in the database.");
                return;
            }

            if (habbo.GetPermissions().HasRight("mod_trade_lock") && !session.GetHabbo().GetPermissions().HasRight("mod_trade_lock_any")) {
                session.SendWhisper("Oops, you cannot trade lock another user ranked 5 or higher.");
                return;
            }

            if (days < 1)
                days = 1;

            if (days > 365)
                days = 365;

            int targetId = habbo.Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                db.UserInfos.Where(u => u.UserId == targetId)
                    .ExecuteUpdate(s => s.SetProperty(u => u.TradingLocked, length)
                        .SetProperty(u => u.TradingLocksCount, u => u.TradingLocksCount + 1));

            if (habbo.GetClient() != null) {
                habbo.TradingLockExpiry = length;
                habbo.GetClient().SendNotification("You have been trade banned for " + days + " day(s)!\r\rReason:\r\r" + message);
            }
        }
    }
}