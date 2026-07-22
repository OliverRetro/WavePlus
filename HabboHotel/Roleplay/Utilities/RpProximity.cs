using Plus.HabboHotel.Rooms;
using System;

namespace Plus.HabboHotel.Roleplay.Utilities
{
    public static class RpProximity
    {
        /// <summary>
        /// Tile-based weapon reach check used by combat.
        ///
        /// Both users may be mid-step when the attack packet lands (the client renders them
        /// between their committed tile and the tile they are stepping onto), so each user is
        /// evaluated at both the tile they are leaving and the tile they are entering. This is
        /// what fixes the "walking side-by-side / criss-cross swings miss" problem.
        ///
        /// The two users are compared PHASE BY PHASE - before-step against before-step, and
        /// after-step against after-step - never as a full cross product. Testing every
        /// combination would pit the attacker's next tile against the target's previous tile,
        /// two positions that never exist at the same instant, which silently stretched reach
        /// by an extra tile (and two, diagonally) whenever both parties were walking.
        ///
        /// Range semantics (Chebyshev rings):
        ///   range 1 → same tile ("inside" the target) or any adjacent tile,
        ///   range 2 → the above plus one full tile of gap, etc.
        /// When <paramref name="allowDiagonal"/> is false the target must additionally share a
        /// row or column with the attacker (cardinal-only weapons).
        /// </summary>
        public static bool IsWithinWeaponRange(RoomUser attacker, RoomUser target, int range, bool allowDiagonal)
        {
            if (attacker == null || target == null)
                return false;

            if (range < 1)
                range = 1;

            // Phase 0 = the tile each user currently occupies.
            // Phase 1 = the tile they are stepping onto (identical to phase 0 when standing still).
            (int X, int Y) attackerFrom = (attacker.X, attacker.Y);
            (int X, int Y) targetFrom = (target.X, target.Y);

            (int X, int Y) attackerTo = StepTile(attacker);
            (int X, int Y) targetTo = StepTile(target);

            return InRange(attackerFrom, targetFrom, range, allowDiagonal)
                || InRange(attackerTo, targetTo, range, allowDiagonal);
        }

        private static bool InRange((int X, int Y) a, (int X, int Y) b, int range, bool allowDiagonal)
        {
            int dx = Math.Abs(a.X - b.X);
            int dy = Math.Abs(a.Y - b.Y);

            // Standing on the exact same tile always counts, whatever the weapon.
            if (dx == 0 && dy == 0)
                return true;

            if (Math.Max(dx, dy) > range)
                return false;

            return allowDiagonal || dx == 0 || dy == 0;
        }

        private static (int X, int Y) StepTile(RoomUser user) =>
            user.SetStep ? (user.SetX, user.SetY) : (user.X, user.Y);
    }
}