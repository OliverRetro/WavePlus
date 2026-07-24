using Plus.HabboHotel.Rooms;
using System;

namespace Plus.HabboHotel.Roleplay.Utilities
{
    public static class RpProximity
    {
        // added reach for non-diagonal attacks
        private const double Tolerance = 0.5;
        // ping compensation
        private const double MaxLagCompensationMs = 300;

        public static bool IsWithinWeaponRange(RoomUser attacker, RoomUser target, int range, bool allowDiagonal)
        {
            if (attacker == null || target == null)
                return false;

            if (range < 0)
                range = 0;

            long at = PlusEnvironment.Now() - (long)LagCompensationMs(attacker);

            (double X, double Y) a = PositionAt(attacker, at);
            (double X, double Y) b = PositionAt(target, at);

            double dx = Math.Abs(a.X - b.X);
            double dy = Math.Abs(a.Y - b.Y);

            if (!allowDiagonal && Math.Min(dx, dy) > Tolerance)
                return false;

            return Math.Max(dx, dy) <= range + Tolerance;
        }

        private static double LagCompensationMs(RoomUser attacker)
        {
            double latency = attacker.GetClient()?.LatencyMs ?? 0;
            return Math.Clamp(latency, 0, MaxLagCompensationMs);
        }

        private static (double X, double Y) PositionAt(RoomUser user, long atMs)
        {
            if (user.StepCommittedMs <= 0)
                return (user.X, user.Y);

            double sinceCommit = atMs - user.StepCommittedMs;

            if (sinceCommit >= 0) {
                if (!user.SetStep)
                    return (user.X, user.Y);

                return Lerp(user.X, user.Y, user.SetX, user.SetY, sinceCommit / RoomUserManager.StepIntervalMs);
            }

            return Lerp(user.PrevX, user.PrevY, user.X, user.Y, 1 + (sinceCommit / RoomUserManager.StepIntervalMs));
        }

        private static (double X, double Y) Lerp(double fromX, double fromY, double toX, double toY, double progress)
        {
            progress = Math.Clamp(progress, 0, 1);
            return (fromX + ((toX - fromX) * progress), fromY + ((toY - fromY) * progress));
        }
    }
}
