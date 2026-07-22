using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Users
{
    internal class SetMessengerInviteStatusEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            bool status = packet.PopBoolean();

            session.GetHabbo().AllowMessengerInvites = status;
            int userId = session.GetHabbo().Id;
            string messengerInvites = PlusEnvironment.BoolToEnum(status);
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.Users.Where(u => u.Id == userId)
                    .ExecuteUpdate(s => s.SetProperty(u => u.IgnoreInvites, messengerInvites));
            }
        }
    }
}