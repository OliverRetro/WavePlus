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
        /// between their committed tile and the tile they are stepping onto), so BOTH tiles of
        /// each moving user count as valid positions. This is what fixes the "walking
        /// side-by-side / criss-cross swings miss" problem: the hit registers if ANY visible
        /// combination of positions is in reach.
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

            Span<(int X, int Y)> attackerTiles = stackalloc (int, int)[2];
            Span<(int X, int Y)> targetTiles = stackalloc (int, int)[2];

            int attackerCount = CandidateTiles(attacker, attackerTiles);
            int targetCount = CandidateTiles(target, targetTiles);

            for (int i = 0; i < attackerCount; i++)
            {
                for (int j = 0; j < targetCount; j++)
                {
                    int dx = Math.Abs(attackerTiles[i].X - targetTiles[j].X);
                    int dy = Math.Abs(attackerTiles[i].Y - targetTiles[j].Y);

                    // Standing on the exact same tile always counts, whatever the weapon.
                    if (dx == 0 && dy == 0)
                        return true;

                    if (Math.Max(dx, dy) > range)
                        continue;

                    if (allowDiagonal || dx == 0 || dy == 0)
                        return true;
                }
            }

            return false;
        }

        /// <summary>Writes the user's committed tile and (when mid-step) their step target tile.</summary>
        private static int CandidateTiles(RoomUser user, Span<(int X, int Y)> tiles)
        {
            tiles[0] = (user.X, user.Y);

            if (user.SetStep && (user.SetX != user.X || user.SetY != user.Y))
            {
                tiles[1] = (user.SetX, user.SetY);
                return 2;
            }

            return 1;
        }
    }
}
