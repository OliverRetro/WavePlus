using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Moderation
{
    internal class ModerationCautionEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().GetPermissions().HasRight("mod_caution"))
                return;

            int userId = packet.PopInt();
            string message = packet.PopString();

            GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
            if (client == null || client.GetHabbo() == null)
                return;

            int targetId = client.GetHabbo().Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                db.UserInfos.Where(u => u.UserId == targetId)
                    .ExecuteUpdate(s => s.SetProperty(u => u.Cautions, u => u.Cautions + 1));

            client.SendNotification(message);
        }
    }
}