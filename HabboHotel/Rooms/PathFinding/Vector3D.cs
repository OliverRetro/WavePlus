namespace Plus.HabboHotel.Rooms.PathFinding
{
    public sealed class Vector3D
    {
        public int X { get; set; }

        public int Y { get; set; }

        public double Z { get; set; }

        public Vector3D()
        {
        }

        public Vector3D(int x, int y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector2D ToVector2D()
        {
            return new Vector2D(X, Y);
        }
    }
}