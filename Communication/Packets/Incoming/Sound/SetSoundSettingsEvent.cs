using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Sound
{
    internal class SetSoundSettingsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            string volume = "";
            for (int i = 0; i < 3; i++) {
                int vol = packet.PopInt();
                if (vol < 0 || vol > 100) {
                    vol = 100;
                }

                if (i < 2)
                    volume += vol + ",";
                else
                    volume += vol;
            }

            int userId = session.GetHabbo().Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.Users.Where(u => u.Id == userId)
                    .ExecuteUpdate(s => s.SetProperty(u => u.Volume, volume));
            }
        }
    }
}