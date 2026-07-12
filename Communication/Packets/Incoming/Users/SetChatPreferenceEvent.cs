using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Users
{
    internal class SetChatPreferenceEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            bool preference = packet.PopBoolean();

            session.GetHabbo().ChatPreference = preference;
            int userId = session.GetHabbo().Id;
            string chatPreference = PlusEnvironment.BoolToEnum(preference);
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                db.users.Where(u => u.id == userId)
                    .ExecuteUpdate(s => s.SetProperty(u => u.chat_preference, chatPreference));
            }
        }
    }
}