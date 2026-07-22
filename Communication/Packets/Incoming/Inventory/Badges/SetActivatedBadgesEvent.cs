using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Inventory.Badges
{
    internal class SetActivatedBadgesEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            session.GetHabbo().GetBadgeComponent().ResetSlots();

            uint badgeUserId = (uint)session.GetHabbo().Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.UserBadges.Where(b => b.UserId == badgeUserId).ExecuteUpdate(s => s.SetProperty(b => b.BadgeSlot, 0));
            }

            for (int i = 0; i < 5; i++) {
                int slot = packet.PopInt();
                string badge = packet.PopString();

                if (badge.Length == 0)
                    continue;

                if (!session.GetHabbo().GetBadgeComponent().HasBadge(badge) || slot < 1 || slot > 5)
                    return;

                session.GetHabbo().GetBadgeComponent().GetBadge(badge).Slot = slot;

                int badgeSlot = slot;
                string badgeId = badge;
                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    db.UserBadges.Where(b => b.BadgeId == badgeId && b.UserId == badgeUserId).ExecuteUpdate(s => s.SetProperty(b => b.BadgeSlot, badgeSlot));
                }
            }

            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.ProfileBadge);

            if (session.GetHabbo().InRoom && PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out Room room))
                room.SendPacket(new HabboUserBadgesComposer(session.GetHabbo()));
            else
                session.SendPacket(new HabboUserBadgesComposer(session.GetHabbo()));
        }
    }
}