using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;
using Plus.Utilities;

namespace Plus.Communication.Packets.Incoming.Rooms.Avatar
{
    internal class ChangeMottoEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            // failsafe for packetloggers
            if (!session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                return;

            if (session.GetHabbo().TimeMuted > 0) {
                session.SendNotification("Oops, you're currently muted - you cannot change your motto.");
                return;
            }

            if ((DateTime.Now - session.GetHabbo().LastMottoUpdateTime).TotalSeconds <= 2.0) {
                session.GetHabbo().MottoUpdateWarnings += 1;
                if (session.GetHabbo().MottoUpdateWarnings >= 25)
                    session.GetHabbo().SessionMottoBlocked = true;
                return;
            }

            if (session.GetHabbo().SessionMottoBlocked)
                return;

            session.GetHabbo().LastMottoUpdateTime = DateTime.Now;

            string newMotto = StringCharFilter.Escape(packet.PopString().Trim());

            if (newMotto.Length > 38)
                newMotto = newMotto.Substring(0, 38);

            if (newMotto == session.GetHabbo().Motto)
                return;

            if (!session.GetHabbo().GetPermissions().HasRight("word_filter_override"))
                newMotto = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(newMotto);

            session.GetHabbo().Motto = newMotto;

            int userId = session.GetHabbo().Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                db.Users.Where(u => u.Id == userId).ExecuteUpdate(s => s.SetProperty(x => x.Motto, newMotto));

            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.ProfileChangeMotto);
            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_Motto", 1);

            if (session.GetHabbo().InRoom) {
                Room room = session.GetHabbo().CurrentRoom;

                RoomUser user = room?.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
                if (user == null || user.GetClient() == null)
                    return;

                room.SendPacket(new UserChangeComposer(user, false));
            }
        }
    }
}