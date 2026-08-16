using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.PathFinding;
using System;

namespace Plus.HabboHotel.Roleplay.Utilities
{
    public static class RpProximity
    {
        // added reach for non-diagonal attacks
        private const double Tolerance = 0.5;
        // ping compensation
        private const double MaxLagCompensationMs = 150;

        /// <summary>
        /// How far apart in height two users may be and still reach each other. Reach is a dome,
        /// not a column: since 3D pathing lets people stand on furniture other people walk under,
        /// tile distance alone would let someone on a floating platform punch, cuff or rob the
        /// person on the floor below them. Matched to <see cref="RoomHeightMap.RequiredHeadroom"/>
        /// on purpose — if they are clear enough overhead that you could walk beneath them, they
        /// are out of reach.
        /// </summary>
        public const double MaxVerticalReach = RoomHeightMap.RequiredHeadroom;

        public static bool IsWithinWeaponRange(RoomUser attacker, RoomUser target, int range, bool allowDiagonal)
        {
            if (attacker == null || target == null)
                return false;

            if (range < 0)
                range = 0;

            long at = PlusEnvironment.MonotonicMs() - (long)LagCompensationMs(attacker);

            (double X, double Y, double Z) a = PositionAt(attacker, at);
            (double X, double Y, double Z) b = PositionAt(target, at);

            if (Math.Abs(a.Z - b.Z) > MaxVerticalReach)
                return false;

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

        private static (double X, double Y, double Z) PositionAt(RoomUser user, long atMs)
        {
            if (user.StepCommittedMs <= 0)
                return (user.X, user.Y, user.Z);

            double sinceCommit = atMs - user.StepCommittedMs;

            // A tile takes as long as the user's walk speed tier says it does, so a fastwalking
            // avatar is interpolated across its shorter step, not a 500ms one.
            double stepDuration = user.StepDurationMs;

            if (sinceCommit >= 0) {
                if (!user.SetStep)
                    return (user.X, user.Y, user.Z);

                return Lerp(user.X, user.Y, user.Z, user.SetX, user.SetY, user.SetZ, sinceCommit / stepDuration);
            }

            return Lerp(user.PrevX, user.PrevY, user.PrevZ, user.X, user.Y, user.Z, 1 + (sinceCommit / stepDuration));
        }

        private static (double X, double Y, double Z) Lerp(double fromX, double fromY, double fromZ, double toX, double toY, double toZ, double progress)
        {
            progress = Math.Clamp(progress, 0, 1);
            return (fromX + ((toX - fromX) * progress), fromY + ((toY - fromY) * progress), fromZ + ((toZ - fromZ) * progress));
        }
    }
}