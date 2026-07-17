namespace Plus.HabboHotel.Rooms.Chat.Styles
{
    public sealed class ChatStyle
    {
        public ChatStyle(int id, string name, string requiredRight, string url, string fontColor)
        {
            Id = id;
            Name = name;
            RequiredRight = requiredRight;
            Url = url;
            FontColor = fontColor;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string RequiredRight { get; set; }

        public string Url { get; set; }

        public string FontColor { get; set; }
    }
}