namespace Plus.HabboHotel.Roleplay.Crime
{
    /// <summary>Punishment applied at a given wanted star level (a row in `rp_crime_penalties`).</summary>
    public class RpCrimePenalty
    {
        public int Id { get; }
        public int StarLevel { get; }

        /// <summary>Credits taken from the offender.</summary>
        public int FineAmount { get; }

        /// <summary>Jail time in minutes.</summary>
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
