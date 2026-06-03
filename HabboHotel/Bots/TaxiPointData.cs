namespace Plus.HabboHotel.Bots
{
    public class TaxiPointData
    {
        public TaxiPointData(int roomId, int x, int y, double z, int rotation, int cost, int delay)
        {
            RoomId = roomId;
            X = x;
            Y = y;
            Z = z;
            Rotation = rotation;
            Cost = cost;
            Delay = delay;
        }

        public int RoomId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public double Z { get; set; }
        public int Rotation { get; set; }
        public int Cost { get; set; }
        public int Delay { get; set; }
    }
}
