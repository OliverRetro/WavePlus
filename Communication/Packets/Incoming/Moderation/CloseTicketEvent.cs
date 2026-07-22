using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;

namespace Plus.Communication.Packets.Incoming.Moderation
{
    internal class CloseTicketEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                return;

            int result = packet.PopInt(); // 1 = useless, 2 = abusive, 3 = resolved
            packet.PopInt(); //junk
            int ticketId = packet.PopInt();

            if (!PlusEnvironment.GetGame().GetModerationManager().TryGetTicket(ticketId, out ModerationTicket ticket))
                return;

            if (ticket.Moderator.Id != session.GetHabbo().Id)
                return;

            GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(ticket.Sender.Id);
            client?.SendPacket(new ModeratorSupportTicketResponseComposer(result));

            if (result == 2) {
                int senderId = ticket.Sender.Id;
                using WavePlusContext db = PlusEnvironment.GetDbContext();
                db.UserInfos.Where(u => u.UserId == senderId)
                    .ExecuteUpdate(s => s.SetProperty(u => u.CfhsAbusive, u => u.CfhsAbusive + 1));
            }

            ticket.Answered = true;
            PlusEnvironment.GetGame().GetClientManager().SendPacket(new ModeratorSupportTicketComposer(session.GetHabbo().Id, ticket), "mod_tool");
        }
    }
}