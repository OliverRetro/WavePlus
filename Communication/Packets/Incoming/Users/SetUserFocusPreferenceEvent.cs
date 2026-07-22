using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Users
{
    internal class SetUserFocusPreferenceEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            bool focusPreference = packet.PopBoolean();

            session.GetHabbo().FocusPreference = focusPreference;
            int userId = session.GetHabbo().Id;
            string focusPref = PlusEnvironment.BoolToEnum(focusPreference);
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.Users.Where(u => u.Id == userId)
                    .ExecuteUpdate(s => s.SetProperty(u => u.FocusPreference, focusPref));
            }
        }
    }
}