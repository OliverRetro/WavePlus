using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Incoming;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Quests;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Messenger;

namespace Plus.HabboHotel.Quests
{
    public class QuestManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(QuestManager));

        private readonly Dictionary<int, Quest> _quests;
        private readonly Dictionary<string, int> _questCount;

        public QuestManager()
        {
            _quests = new Dictionary<int, Quest>();
            _questCount = new Dictionary<string, int>();
        }

        public void Init()
        {
            if (_quests.Count > 0)
                _quests.Clear();

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var rows = db.quests.Select(q => new
                {
                    q.id,
                    q.type,
                    q.level_num,
                    q.goal_type,
                    q.goal_data,
                    q.action,
                    q.pixel_reward,
                    q.data_bit,
                    q.reward_type,
                    q.timestamp_unlock,
                    q.timestamp_lock
                }).ToList();

                foreach (var dRow in rows)
                {
                    int id = Convert.ToInt32(dRow.id);
                    string category = Convert.ToString(dRow.type);
                    int num = Convert.ToInt32(dRow.level_num);
                    int type = Convert.ToInt32(dRow.goal_type);
                    int goalData = Convert.ToInt32(dRow.goal_data);
                    string name = Convert.ToString(dRow.action);
                    int reward = Convert.ToInt32(dRow.pixel_reward);
                    string dataBit = Convert.ToString(dRow.data_bit);
                    int rewardType = Convert.ToInt32(dRow.reward_type);
                    int time = Convert.ToInt32(dRow.timestamp_unlock);
                    int locked = Convert.ToInt32(dRow.timestamp_lock);

                    _quests.Add(id, new Quest(id, category, num, (QuestType) type, goalData, name, reward, dataBit, rewardType, time, locked));
                    AddToCounter(category);
                }
            }

            Log.Info("Quest Manager -> LOADED");
        }

        private void AddToCounter(string category)
        {
            if (_questCount.TryGetValue(category, out int count)) {
                _questCount[category] = count + 1;
            } else {
                _questCount.Add(category, 1);
            }
        }

        public Quest GetQuest(int id)
        {
            _quests.TryGetValue(id, out Quest quest);
            return quest;
        }

        public int GetAmountOfQuestsInCategory(string category)
        {
            _questCount.TryGetValue(category, out int count);
            return count;
        }

        public void ProgressUserQuest(GameClient session, QuestType type, int data = 0)
        {
            if (session == null || session.GetHabbo() == null || session.GetHabbo().GetStats().QuestId <= 0) {
                return;
            }

            Quest quest = GetQuest(session.GetHabbo().GetStats().QuestId);

            if (quest == null || quest.GoalType != type) {
                return;
            }

            int currentProgress = session.GetHabbo().GetQuestProgress(quest.Id);
            int totalProgress = currentProgress;
            bool completeQuest = false;

            switch (type)
            {
                default:
                    totalProgress++;

                    if (totalProgress >= quest.GoalData) {
                        completeQuest = true;
                    }

                    break;

                case QuestType.ExploreFindItem:
                    if (data != quest.GoalData)
                        return;

                    totalProgress = Convert.ToInt32(quest.GoalData);
                    completeQuest = true;
                    break;

                case QuestType.StandOn:
                    if (data != quest.GoalData)
                        return;

                    totalProgress = Convert.ToInt32(quest.GoalData);
                    completeQuest = true;
                    break;

                case QuestType.XmasParty:
                    totalProgress++;
                    if (totalProgress == quest.GoalData)
                        completeQuest = true;
                    break;

                case QuestType.GiveItem:
                    if (data != quest.GoalData)
                        return;

                    totalProgress = Convert.ToInt32(quest.GoalData);
                    completeQuest = true;
                    break;
            }

            uint progressUserId = (uint)session.GetHabbo().Id;
            uint progressQuestId = (uint)quest.Id;
            int progressStatsId = session.GetHabbo().Id;
            int progressValue = totalProgress;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                db.user_quests.Where(q => q.user_id == progressUserId && q.quest_id == progressQuestId).ExecuteUpdate(s => s.SetProperty(q => q.progress, progressValue));

                if (completeQuest)
                    db.user_stats.Where(s => s.id == progressStatsId).ExecuteUpdate(x => x.SetProperty(s => s.quest_id, 0u));
            }

            session.GetHabbo().Quests[session.GetHabbo().GetStats().QuestId] = totalProgress;
            session.SendPacket(new QuestStartedComposer(session, quest));

            if (completeQuest) {
                session.GetHabbo().GetMessenger().BroadcastAchievement(session.GetHabbo().Id, MessengerEventTypes.QuestCompleted, quest.Category + "." + quest.Name);

                session.GetHabbo().GetStats().QuestId = 0;
                session.GetHabbo().QuestLastCompleted = quest.Id;
                session.SendPacket(new QuestCompletedComposer(session, quest));
                session.GetHabbo().Duckets += quest.Reward;
                session.SendPacket(new HabboActivityPointNotificationComposer(session.GetHabbo().Duckets, quest.Reward));
                GetList(session, null);
            }
        }

        public Quest GetNextQuestInSeries(string category, int number)
        {
            foreach (Quest quest in _quests.Values)
            {
                if (quest.Category == category && quest.Number == number) {
                    return quest;
                }
            }

            return null;
        }

        public void GetList(GameClient session, ClientPacket message)
        {
            Dictionary<string, int> userQuestGoals = new();
            Dictionary<string, Quest> userQuests = new();

            foreach (Quest quest in _quests.Values.ToList())
            {
                if (quest.Category.Contains("xmas2012"))
                    continue;

                if (!userQuestGoals.ContainsKey(quest.Category)) {
                    userQuestGoals.Add(quest.Category, 1);
                    userQuests.Add(quest.Category, null);
                }

                if (quest.Number >= userQuestGoals[quest.Category]) {
                    int userProgress = session.GetHabbo().GetQuestProgress(quest.Id);

                    if (session.GetHabbo().GetStats().QuestId != quest.Id && userProgress >= quest.GoalData) {
                        userQuestGoals[quest.Category] = quest.Number + 1;
                    }
                }
            }

            foreach (Quest quest in _quests.Values.ToList())
            {
                foreach (var goal in userQuestGoals)
                {
                    if (quest.Category.Contains("xmas2012"))
                        continue;

                    if (quest.Category == goal.Key && quest.Number == goal.Value) {
                        userQuests[goal.Key] = quest;
                        break;
                    }
                }
            }

            session.SendPacket(new QuestListComposer(session, (message != null), userQuests));
        }

        public void QuestReminder(GameClient session, int questId)
        {
            Quest quest = GetQuest(questId);
            if (quest == null)
                return;

            session.SendPacket(new QuestStartedComposer(session, quest));
        }
    }
}