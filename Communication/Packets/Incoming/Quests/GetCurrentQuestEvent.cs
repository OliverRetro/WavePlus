using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Quests;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Quests
{
    internal class GetCurrentQuestEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().InRoom)
                return;

            HabboHotel.Quests.Quest userQuest = PlusEnvironment.GetGame().GetQuestManager().GetQuest(session.GetHabbo().QuestLastCompleted);
            HabboHotel.Quests.Quest nextQuest = PlusEnvironment.GetGame().GetQuestManager().GetNextQuestInSeries(userQuest.Category, userQuest.Number + 1);

            if (nextQuest == null)
                return;

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.UserQuests.Upsert(new UserQuestEntity
                {
                    UserId = (uint)session.GetHabbo().Id,
                    QuestId = (uint)nextQuest.Id
                }).Run();
                db.UserStats.Where(s => s.Id == session.GetHabbo().Id)
                    .ExecuteUpdate(s => s.SetProperty(x => x.QuestId, (uint)nextQuest.Id));
            }

            session.GetHabbo().GetStats().QuestId = nextQuest.Id;
            PlusEnvironment.GetGame().GetQuestManager().GetList(session, null);
            session.SendPacket(new QuestStartedComposer(session, nextQuest));
        }
    }
}