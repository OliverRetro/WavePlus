namespace Plus.HabboHotel.Roleplay.Crime
{
    public class RpCrimePenalty
    {
        public int Id { get; }
        public int StarLevel { get; }

        public int FineAmount { get; }

        public int JailTimeMinutes { get; }

        public RpCrimePenalty(int id, int starLevel, int fineAmount, int jailTimeMinutes)
        {
            Id = id;
            StarLevel = starLevel;
            FineAmount = fineAmount;
            JailTimeMinutes = jailTimeMinutes;
        }
    }
}