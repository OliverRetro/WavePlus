using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Quests;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;

namespace Plus.Communication.Packets.Incoming.Quests
{
    internal class CancelQuestEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            Quest quest = PlusEnvironment.GetGame().GetQuestManager().GetQuest(session.GetHabbo().GetStats().QuestId);
            if (quest == null)
                return;

            uint questUserId = (uint)session.GetHabbo().Id;
            uint questId = (uint)quest.Id;
            int statsId = session.GetHabbo().Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.UserQuests.Where(q => q.UserId == questUserId && q.QuestId == questId).ExecuteDelete();
                db.UserStats.Where(s => s.Id == statsId).ExecuteUpdate(x => x.SetProperty(s => s.QuestId, 0u));
            }

            session.GetHabbo().GetStats().QuestId = 0;
            session.SendPacket(new QuestAbortedComposer());

            PlusEnvironment.GetGame().GetQuestManager().GetList(session, null);
        }
    }
}