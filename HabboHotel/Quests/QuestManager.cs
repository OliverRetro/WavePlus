using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using log4net;
using Plus.Communication.Packets.Incoming;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Quests;
using Plus.Database.Interfaces;
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

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`type`,`level_num`,`goal_type`,`goal_data`,`action`,`pixel_reward`,`data_bit`,`reward_type`,`timestamp_unlock`,`timestamp_lock` FROM `quests`");
                DataTable dTable = dbClient.GetTable();

                if (dTable != null) {
                    foreach (DataRow dRow in dTable.Rows)
                    {
                        int id = Convert.ToInt32(dRow["id"]);
                        string category = Convert.ToString(dRow["type"]);
                        int num = Convert.ToInt32(dRow["level_num"]);
                        int type = Convert.ToInt32(dRow["goal_type"]);
                        int goalData = Convert.ToInt32(dRow["goal_data"]);
                        string name = Convert.ToString(dRow["action"]);
                        int reward = Convert.ToInt32(dRow["pixel_reward"]);
                        string dataBit = Convert.ToString(dRow["data_bit"]);
                        int rewardType = Convert.ToInt32(dRow["reward_type"].ToString());
                        int time = Convert.ToInt32(dRow["timestamp_unlock"]);
                        int locked = Convert.ToInt32(dRow["timestamp_lock"]);

                        _quests.Add(id, new Quest(id, category, num, (QuestType) type, goalData, name, reward, dataBit, rewardType, time, locked));
                        AddToCounter(category);
                    }
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

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `user_quests` SET `progress` = '" + totalProgress + "' WHERE `user_id` = '" + session.GetHabbo().Id + "' AND `quest_id` = '" + quest.Id + "' LIMIT 1");

                if (completeQuest)
                    dbClient.RunQuery("UPDATE `user_stats` SET `quest_id` = '0' WHERE `id` = '" + session.GetHabbo().Id + "' LIMIT 1");
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