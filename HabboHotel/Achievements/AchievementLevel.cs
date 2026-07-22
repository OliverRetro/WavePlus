namespace Plus.HabboHotel.Achievements
{
    public struct AchievementLevel
    {
        public int Level { get; }
        public int Requirement { get; }
        public int RewardAmount { get; }
        // Currency the RewardAmount is paid in: -1 = credits, 5 = diamonds, 0 = duckets.
        public int PointsType { get; }
        public int RewardPoints { get; }

        public AchievementLevel(int level, int rewardAmount, int pointsType, int rewardPoints, int requirement)
        {
            Level = level;
            RewardAmount = rewardAmount;
            PointsType = pointsType;
            RewardPoints = rewardPoints;
            Requirement = requirement;
        }
    }
}