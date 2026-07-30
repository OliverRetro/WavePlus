namespace Plus.HabboHotel.Roleplay.Crime
{
    public class RpCrime
    {
        public int Id { get; }
        public string Tag { get; }
        public string Name { get; }

        public int Severity { get; }

        public int TimeActiveMinutes { get; }
        public bool AutoCharge { get; }

        public string Note { get; }

        public RpCrime(int id, string tag, string name, int severity, int timeActiveMinutes, bool autoCharge, string note)
        {
            Id = id;
            Tag = tag;
            Name = name;
            Severity = severity;
            TimeActiveMinutes = timeActiveMinutes;
            AutoCharge = autoCharge;
            Note = note;
        }
    }
}