using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;
using Plus.HabboHotel.Users;
using Plus.Utilities;

namespace Plus.Communication.Packets.Incoming.Moderation
{
    internal class SubmitNewTicketEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null)
                return;

            // Run a quick check to see if we have any existing tickets.
            if (PlusEnvironment.GetGame().GetModerationManager().UserHasTickets(session.GetHabbo().Id)) {
                ModerationTicket pendingTicket = PlusEnvironment.GetGame().GetModerationManager().GetTicketBySenderId(session.GetHabbo().Id);
                if (pendingTicket != null) {
                    session.SendPacket(new CallForHelpPendingCallsComposer(pendingTicket));
                    return;
                }
            }

            List<string> chats = new();

            string message = StringCharFilter.Escape(packet.PopString().Trim());
            int category = packet.PopInt();
            int reportedUserId = packet.PopInt();
            int type = packet.PopInt(); // Unsure on what this actually is.

            Habbo reportedUser = PlusEnvironment.GetHabboById(reportedUserId);
            if (reportedUser == null) {
                // User doesn't exist.
                return;
            }

            int messageCount = packet.PopInt();
            for (int i = 0; i < messageCount; i++) {
                packet.PopInt();
                chats.Add(packet.PopString());
            }

            ModerationTicket ticket = new(1, type, category, UnixTimestamp.GetNow(), 1, session.GetHabbo(), reportedUser, message, session.GetHabbo().CurrentRoom, chats);
            if (!PlusEnvironment.GetGame().GetModerationManager().TryAddTicket(ticket))
                return;

            // TODO: Come back to this (persisting the moderation_tickets row).
            int senderId = session.GetHabbo().Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                db.UserInfos.Where(u => u.UserId == senderId)
                    .ExecuteUpdate(s => s.SetProperty(u => u.Cfhs, u => u.Cfhs + 1));

            PlusEnvironment.GetGame().GetClientManager().ModAlert("A new support ticket has been submitted!");
            PlusEnvironment.GetGame().GetClientManager().SendPacket(new ModeratorSupportTicketComposer(session.GetHabbo().Id, ticket), "mod_tool");
        }
    }
}