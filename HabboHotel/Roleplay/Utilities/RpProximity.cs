using Plus.HabboHotel.Rooms;
using System;

namespace Plus.HabboHotel.Roleplay.Utilities
{
    public static class RpProximity
    {
        //The reach of a bare fist / a plain "you must be beside them" interaction.
        public const double AdjacentRange = 1.5;

        public static bool IsWithinRange(RoomUser a, RoomUser b, double range)
        {
            if (a == null || b == null)
                return false;

            (double ax, double ay) = RenderedPosition(a);
            (double bx, double by) = RenderedPosition(b);

            double dx = ax - bx;
            double dy = ay - by;

            return Math.Sqrt((dx * dx) + (dy * dy)) <= range;
        }

        private static (double X, double Y) RenderedPosition(RoomUser user)
        {
            if (!user.IsWalking || (user.SetX == user.X && user.SetY == user.Y))
                return (user.X, user.Y);

            return ((user.X + user.SetX) / 2.0, (user.Y + user.SetY) / 2.0);
        }
    }
}
