namespace Plus.HabboHotel.Camera
{
    public class PendingPhoto
    {
        public int UserId { get; }
        public int RoomId { get; }
        public int Timestamp { get; }
        public string Filename { get; }
        public bool Published { get; set; }

        public PendingPhoto(int userId, int roomId, int timestamp, string filename)
        {
            UserId = userId;
            RoomId = roomId;
            Timestamp = timestamp;
            Filename = filename;
        }
    }
}