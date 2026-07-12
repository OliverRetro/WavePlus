using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Quests;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;

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

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                db.user_quests.Upsert(new user_quest
                {
                    user_id = (uint)session.GetHabbo().Id,
                    quest_id = (uint)quest.Id
                }).Run();
                db.user_stats.Where(s => s.id == session.GetHabbo().Id)
                    .ExecuteUpdate(s => s.SetProperty(x => x.quest_id, (uint)quest.Id));
            }

            session.GetHabbo().GetStats().QuestId = quest.Id;
            PlusEnvironment.GetGame().GetQuestManager().GetList(session, null);
            session.SendPacket(new QuestStartedComposer(session, quest));
        }
    }
}