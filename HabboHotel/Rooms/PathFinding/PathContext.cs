using System.Collections.Generic;

namespace Plus.HabboHotel.Rooms.PathFinding
{
    /// <summary>
    /// Everything the pathfinder is allowed to know about a room, captured on the room thread.
    ///
    /// It exists so pathfinding is a pure function of a snapshot: no live item lists, no room
    /// locks, no <see cref="RoomUser"/> reads. That is what makes it safe to run the full path
    /// off-thread while the user has already started walking.
    /// </summary>
    public sealed class PathContext
    {
        /// <summary>No user on the tile.</summary>
        public const byte OccupancyNone = 0;

        /// <summary>A walking user is on the tile — you may finish your walk there.</summary>
        public const byte OccupancyWalking = 1;

        /// <summary>A standing user is on the tile — it is off limits entirely.</summary>
        public const byte OccupancyStanding = 2;

        public RoomHeightMap HeightMap { get; init; }

        public bool DiagonalEnabled { get; init; }

        /// <summary>Staff/override walking: nothing blocks, nothing is too high.</summary>
        public bool AllowOverride { get; init; }

        /// <summary>Horses ignore the climb/fall limits, exactly as they did before.</summary>
        public bool IgnoreHeightLimits { get; init; }

        /// <summary>True when the room blocks tiles that already hold a user.</summary>
        public bool UsersBlockTiles { get; init; }

        /// <summary>Per-tile occupancy snapshot, or null when nobody blocks anything.</summary>
        public byte[] Occupancy { get; init; }

        /// <summary>Guild groups whose gates this user may walk through, or null for none.</summary>
        public HashSet<int> OpenGateGroups { get; init; }

        /// <summary>
        /// Whether a user may set foot on one specific surface of one specific tile.
        /// </summary>
        public bool CanEnter(int tile, int level, bool isFinalStep)
        {
            if (AllowOverride)
                return true;

            byte state = HeightMap.StateAt(tile, level);
            bool stateAllows = state == RoomHeightMap.StateOpen ||
                               ((state == RoomHeightMap.StateSeat || state == RoomHeightMap.StateLastStep) && isFinalStep);

            if (!stateAllows) {
                // A guild gate is the one thing that rescues an otherwise closed square, and only
                // for members of the group that owns it.
                int gateGroup = HeightMap.GateGroupAt(tile, level);
                if (gateGroup == 0 || OpenGateGroups == null || !OpenGateGroups.Contains(gateGroup))
                    return false;
            }

            if (UsersBlockTiles && Occupancy != null && tile < Occupancy.Length) {
                byte occupancy = Occupancy[tile];

                if (occupancy == OccupancyStanding)
                    return false;

                if (occupancy == OccupancyWalking && !isFinalStep)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Whether this tile can be stood on at all. False for the likes of a table or a wall
        /// block, which are targets to walk up to rather than onto.
        /// </summary>
        public bool HasEnterableLevel(int tile)
        {
            int count = HeightMap.LevelCountAt(tile);

            for (int level = 0; level < count; level++) {
                if (CanEnter(tile, level, true))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Picks the surface of <paramref name="tile"/> a user standing at <paramref name="fromHeight"/>
        /// would actually step onto — the reachable one closest to their current height, so walking
        /// under a floating item is preferred over vaulting on top of it.
        /// </summary>
        public int SelectLevel(int tile, double fromHeight, bool isFinalStep)
        {
            int count = HeightMap.LevelCountAt(tile);
            int best = -1;
            double bestDelta = double.MaxValue;

            for (int level = 0; level < count; level++) {
                if (!CanEnter(tile, level, isFinalStep))
                    continue;

                double surface = HeightMap.SurfaceAt(tile, level);

                if (!AllowOverride && !IgnoreHeightLimits && !RoomHeightMap.CanTransition(fromHeight, surface))
                    continue;

                double delta = surface >= fromHeight ? surface - fromHeight : fromHeight - surface;
                if (delta < bestDelta) {
                    bestDelta = delta;
                    best = level;
                }
            }

            return best;
        }
    }
}
