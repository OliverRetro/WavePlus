using Plus.Utilities;

namespace Plus.HabboHotel.Quests
{
    public class Quest
    {
        public int Id { get; }
        public string Category { get; }
        public string DataBit { get; }
        public int GoalData { get; }
        public QuestType GoalType { get; }
        public bool HasEnded { get; }
        public string Name { get; }
        public int Number { get; }
        public int Reward { get; }
        public int RewardType { get; }
        public int TimeUnlock { get; }

        public Quest(int id, string category, int number, QuestType goalType, int goalData, string name, int reward, string dataBit, int rewardType, int timeUnlock, int timeLock)
        {
            Id = id;
            Category = category;
            Number = number;
            GoalType = goalType;
            GoalData = goalData;
            Name = name;
            Reward = reward;
            DataBit = dataBit;
            RewardType = rewardType;
            TimeUnlock = timeUnlock;
            HasEnded = timeLock >= UnixTimestamp.GetNow() && timeLock > 0;
        }

        public string ActionName => QuestTypeUtility.GetString(GoalType);

        public bool IsCompleted(int progress)
        {
            switch (GoalType) {
                default:
                    return progress >= GoalData;
                case QuestType.ExploreFindItem:
                    return progress >= 1;
            }
        }
    }
}