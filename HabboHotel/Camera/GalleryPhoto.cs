namespace Plus.HabboHotel.Camera
{
    public class GalleryPhoto
    {
        public GalleryPhoto(string url, int timestamp, int roomId)
        {
            Url = url;
            Timestamp = timestamp;
            RoomId = roomId;
        }
        public string Url { get; }
        public int Timestamp { get; }
        public int RoomId { get; }
    }
}