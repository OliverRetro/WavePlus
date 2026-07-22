using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Quests;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.GameClients;
// Disambiguates the bare name from the scaffolded EF entity Plus.Database.EF.Entities.Quest.
using Quest = Plus.HabboHotel.Quests.Quest;

namespace Plus.Communication.Packets.Incoming.Quests
{
    internal class StartQuestEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int questId = packet.PopInt();

            Quest quest = PlusEnvironment.GetGame().GetQuestManager().GetQuest(questId);
            if (quest == null)
                return;

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.UserQuests.Upsert(new UserQuestEntity
                {
                    UserId = (uint)session.GetHabbo().Id,
                    QuestId = (uint)quest.Id
                }).Run();
                db.UserStats.Where(s => s.Id == session.GetHabbo().Id)
                    .ExecuteUpdate(s => s.SetProperty(x => x.QuestId, (uint)quest.Id));
            }

            session.GetHabbo().GetStats().QuestId = quest.Id;
            PlusEnvironment.GetGame().GetQuestManager().GetList(session, null);
            session.SendPacket(new QuestStartedComposer(session, quest));
        }
    }
}