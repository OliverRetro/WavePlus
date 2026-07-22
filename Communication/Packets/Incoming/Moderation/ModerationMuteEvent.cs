using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Moderation
{
    internal class ModerationMuteEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().GetPermissions().HasRight("mod_mute"))
                return;

            int userId = packet.PopInt();
            packet.PopString(); //message
            double length = packet.PopInt() * 60;
            packet.PopString(); //unk1
            packet.PopString(); //unk2

            Habbo habbo = PlusEnvironment.GetHabboById(userId);
            if (habbo == null) {
                session.SendWhisper("An error occoured whilst finding that user in the database.");
                return;
            }

            if (habbo.GetPermissions().HasRight("mod_mute") && !session.GetHabbo().GetPermissions().HasRight("mod_mute_any")) {
                session.SendWhisper("Oops, you cannot mute that user.");
                return;
            }

            int targetId = habbo.Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                db.Users.Where(u => u.Id == targetId)
                    .ExecuteUpdate(s => s.SetProperty(u => u.TimeMuted, length));

            if (habbo.GetClient() != null) {
                habbo.TimeMuted = length;
                habbo.GetClient().SendNotification("You have been muted by a moderator for " + length + " seconds!");
            }
        }
    }
}