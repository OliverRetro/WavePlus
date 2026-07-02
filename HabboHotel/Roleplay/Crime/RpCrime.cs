namespace Plus.HabboHotel.Roleplay.Crime
{
    /// <summary>Definition of a committable crime (a row in `rp_crimes`).</summary>
    public class RpCrime
    {
        public int Id { get; }
        public string Tag { get; }
        public string Name { get; }

        /// <summary>Stars gained when this crime is committed.</summary>
        public int Severity { get; }

        /// <summary>How long (minutes) the crime stays chargeable before it expires.</summary>
        public int TimeActiveMinutes { get; }

        /// <summary>Whether the stars are applied to the visible wanted level automatically.</summary>
        public bool AutomaticReport { get; }

        /// <summary>Whether committing this crime alerts on-duty police.</summary>
        public bool PoliceAlert { get; }

        public string Note { get; }

        public RpCrime(int id, string tag, string name, int severity, int timeActiveMinutes, bool automaticReport, bool policeAlert, string note)
        {
            Id = id;
            Tag = tag;
            Name = name;
            Severity = severity;
            TimeActiveMinutes = timeActiveMinutes;
            AutomaticReport = automaticReport;
            PoliceAlert = policeAlert;
            Note = note;
        }
    }
}
