namespace Plus.HabboHotel.Items.Televisions
{
    public class TelevisionItem
    {
        public TelevisionItem(int id, string youTubeId, string title, string description, bool enabled)
        {
            Id = id;
            YouTubeId = youTubeId;
            Title = title;
            Description = description;
            Enabled = enabled;
        }

        public int Id { get; }

        public string YouTubeId { get; }

        public string Title { get; }

        public string Description { get; }

        public bool Enabled { get; }
    }
}