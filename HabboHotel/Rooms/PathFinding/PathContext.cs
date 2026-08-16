using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Plus.HabboHotel.Rooms.PathFinding
{
    /// <summary>
    /// Everything the pathfinder is allowed to know about a room, captured on the room thread.
    ///
    /// It exists so pathfinding is a pure function of a snapshot: no live item lists, no room
    /// locks, no <see cref="RoomUser"/> reads. A search is therefore cheap enough, and free enough
    /// of contention, to run start to finish on the room thread the moment a click lands.
    /// </summary>
    public sealed class PathContext
    {
        /// <summary>No user on the tile.</summary>
        public const byte OccupancyNone = 0;

        /// <summary>A walking user is on the tile — you may finish your walk there.</summary>
        public const byte OccupancyWalking = 1;

        /// <summary>A standing user is on the tile — it is off limits entirely.</summary>
        public const byte OccupancyStanding = 2;

        private readonly RoomHeightMap _heightMap;
        private readonly bool _threeDEnabled;

        private readonly bool _flattens;

        public RoomHeightMap HeightMap
        {
            get => _heightMap;
            init
            {
                _heightMap = value;
                _flattens = !_threeDEnabled && value != null && value.LevelDepth > 1;
            }
        }

        public bool DiagonalEnabled { get; init; }

        public bool ThreeDEnabled
        {
            get => _threeDEnabled;
            init
            {
                _threeDEnabled = value;
                _flattens = !value && _heightMap != null && _heightMap.LevelDepth > 1;
            }
        }

        public int SearchDepth => ThreeDEnabled ? HeightMap.LevelDepth : 1;
        private bool Flattens => _flattens;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int UsableLevelCount(int tile)
        {
            int count = HeightMap.LevelCountAt(tile);

            if (count == 0)
                return 0;

            return Flattens ? 1 : count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ResolveLevel(int tile, int level)
        {
            if (!Flattens)
                return level;

            int count = HeightMap.LevelCountAt(tile);

            return count <= 0 ? 0 : count - 1;
        }

        /// <summary>Staff/override walking: nothing blocks, nothing is too high.</summary>
        public bool AllowOverride { get; init; }

        /// <summary>Horses ignore the climb/fall limits, exactly as they did before.</summary>
        public bool IgnoreHeightLimits { get; init; }

        /// <summary>True when the room blocks tiles that already hold a user.</summary>
        public bool UsersBlockTiles { get; init; }

        /// <summary>Per-tile occupancy snapshot, or null when nobody blocks anything.</summary>
        public byte[] Occupancy { get; init; }

        public int IgnoreTile { get; init; } = -1;

        /// <summary>Guild groups whose gates this user may walk through, or null for none.</summary>
        public HashSet<int> OpenGateGroups { get; init; }

        /// <summary>Height of one search-space surface of a tile.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double SurfaceOf(int tile, int level) => HeightMap.SurfaceAt(tile, ResolveLevel(tile, level));

        /// <summary>
        /// Whether a user may set foot on one specific surface of one specific tile.
        /// </summary>
        public bool CanEnter(int tile, int level, bool isFinalStep)
        {
            if (AllowOverride)
                return true;

            level = ResolveLevel(tile, level);

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

            if (UsersBlockTiles && Occupancy != null && tile != IgnoreTile && tile < Occupancy.Length) {
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
            int count = UsableLevelCount(tile);

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
            int count = UsableLevelCount(tile);
            int best = -1;
            double bestDelta = double.MaxValue;

            for (int level = 0; level < count; level++) {
                if (!CanEnter(tile, level, isFinalStep))
                    continue;

                double surface = SurfaceOf(tile, level);

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