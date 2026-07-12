using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Quests;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;

namespace Plus.Communication.Packets.Incoming.Quests
{
    internal class GetCurrentQuestEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().InRoom)
                return;

            Quest userQuest = PlusEnvironment.GetGame().GetQuestManager().GetQuest(session.GetHabbo().QuestLastCompleted);
            Quest nextQuest = PlusEnvironment.GetGame().GetQuestManager().GetNextQuestInSeries(userQuest.Category, userQuest.Number + 1);

            if (nextQuest == null)
                return;

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                db.user_quests.Upsert(new user_quest
                {
                    user_id = (uint)session.GetHabbo().Id,
                    quest_id = (uint)nextQuest.Id
                }).Run();
                db.user_stats.Where(s => s.id == session.GetHabbo().Id)
                    .ExecuteUpdate(s => s.SetProperty(x => x.quest_id, (uint)nextQuest.Id));
            }

            session.GetHabbo().GetStats().QuestId = nextQuest.Id;
            PlusEnvironment.GetGame().GetQuestManager().GetList(session, null);
            session.SendPacket(new QuestStartedComposer(session, nextQuest));
        }
    }
}