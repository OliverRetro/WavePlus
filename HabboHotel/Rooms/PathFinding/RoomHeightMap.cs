using System;
using System.Collections.Generic;

namespace Plus.HabboHotel.Rooms.PathFinding
{
    /// <summary>
    /// How a piece of furniture behaves vertically, which is all the height map cares about.
    /// </summary>
    public enum FurnitureFootprint
    {
        Solid,
        Walkable,
        // walkable - no walkthrough, but can walk on.
        WalkableStopOnly,
        // no walk through
        Seat,
        // group walk through
        GuildGate
    }

    public sealed class RoomHeightMap
    {
        public const double MaxClimbHeight = 1.5;
        public const double MaxFallHeight = 2.2;

        // 3D - to walk below this height is required.
        public const double RequiredHeadroom = 2.5;
        public const int MaxLevels = 4;

        public const double Epsilon = 0.01;

        public const byte StateBlocked = 0;
        public const byte StateOpen = 1;
        public const byte StateSeat = 2;
        public const byte StateLastStep = 3;

        private const double MinSolidThickness = 1.0;
        private const double PlatformThickness = 0.25;

        private readonly byte[] _levelCount;
        private readonly double[] _surface;
        private readonly byte[] _state;
        private readonly int[] _gateGroup;

        public int Width { get; }

        public int Height { get; }
        public int[] GateGroupIds { get; private set; } = Array.Empty<int>();

        // 2D or 3D
        public int LevelDepth { get; private set; } = 1;

        // true when no tile stacks surfaces, i.e. the room is a flat 2D grid.
        public bool IsFlat => LevelDepth <= 1;

        private RoomHeightMap(int width, int height)
        {
            Width = width;
            Height = height;

            int size = width * height;
            _levelCount = new byte[size];
            _surface = new double[size * MaxLevels];
            _state = new byte[size * MaxLevels];
            _gateGroup = new int[size * MaxLevels];
        }

        public RoomHeightMap Clone()
        {
            var clone = new RoomHeightMap(Width, Height);

            Array.Copy(_levelCount, clone._levelCount, _levelCount.Length);
            Array.Copy(_surface, clone._surface, _surface.Length);
            Array.Copy(_state, clone._state, _state.Length);
            Array.Copy(_gateGroup, clone._gateGroup, _gateGroup.Length);

            clone.GateGroupIds = GateGroupIds;
            clone.LevelDepth = LevelDepth;

            return clone;
        }

        public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

        public int TileIndex(int x, int y) => (y * Width) + x;

        public int LevelCountAt(int tile) => tile < 0 || tile >= _levelCount.Length ? 0 : _levelCount[tile];

        public int LevelCountAt(int x, int y) => InBounds(x, y) ? _levelCount[TileIndex(x, y)] : 0;

        public double SurfaceAt(int tile, int level) => _surface[(tile * MaxLevels) + level];

        public byte StateAt(int tile, int level) => _state[(tile * MaxLevels) + level];

        public int GateGroupAt(int tile, int level) => _gateGroup[(tile * MaxLevels) + level];

        public static bool CanTransition(double fromHeight, double toHeight)
        {
            double delta = toHeight - fromHeight;

            if (delta > MaxClimbHeight + Epsilon)
                return false;

            return -delta <= MaxFallHeight + Epsilon;
        }

        public int FindLevel(int x, int y, double height, double tolerance = 0.4)
        {
            if (!InBounds(x, y))
                return -1;

            int tile = TileIndex(x, y);
            int count = _levelCount[tile];
            int best = -1;
            double bestDelta = double.MaxValue;

            for (int level = 0; level < count; level++) {
                double delta = Math.Abs(_surface[(tile * MaxLevels) + level] - height);
                if (delta < bestDelta) {
                    bestDelta = delta;
                    best = level;
                }
            }

            return bestDelta <= tolerance ? best : -1;
        }

        public double SurfaceNear(int x, int y, double fallback)
        {
            if (!InBounds(x, y))
                return fallback;

            int tile = TileIndex(x, y);
            int count = _levelCount[tile];
            double best = fallback;
            double bestDelta = double.MaxValue;

            for (int level = 0; level < count; level++) {
                double surface = _surface[(tile * MaxLevels) + level];
                double delta = Math.Abs(surface - fallback);
                if (delta < bestDelta) {
                    bestDelta = delta;
                    best = surface;
                }
            }

            return best;
        }

        public double TopSurface(int x, int y, double fallback)
        {
            if (!InBounds(x, y))
                return fallback;

            int tile = TileIndex(x, y);
            int count = _levelCount[tile];

            return count == 0 ? fallback : _surface[(tile * MaxLevels) + count - 1];
        }

        public sealed class Builder
        {
            private readonly RoomHeightMap _map;
            private readonly List<Solid> _solids = new();
            private readonly List<Surface> _surfaces = new();
            private readonly HashSet<int> _gateGroups = new();

            public Builder(int width, int height)
            {
                _map = new RoomHeightMap(width, height);
            }

            public Builder(RoomHeightMap target)
            {
                _map = target;

                foreach (int groupId in target.GateGroupIds)
                    _gateGroups.Add(groupId);
            }

            public void AddSolid(int ownerId, double bottom, double top, bool enforceMinimumThickness)
            {
                if (enforceMinimumThickness)
                    top = Math.Max(top, bottom + MinSolidThickness);

                if (top <= bottom + Epsilon)
                    return;

                _solids.Add(new Solid(ownerId, bottom, top));
            }

            public void AddFurniture(int ownerId, FurnitureFootprint footprint, double baseZ, double topZ, double furnitureHeight, double floorHeight, int gateGroupId)
            {
                switch (footprint) {
                    case FurnitureFootprint.GuildGate:
                        // Group membership decides this one, so it never occupies space: it is a
                        // surface tagged with its group and the pathfinder turns non-members away.
                        AddSurface(ownerId, Math.Max(floorHeight, baseZ), StateOpen, gateGroupId);
                        break;

                    case FurnitureFootprint.Seat:
                        // You sit/lie at the item's own base height — the same deduction the legacy
                        // SqAbsoluteHeight makes.
                        AddSolid(ownerId, baseZ, topZ, false);
                        AddSurface(ownerId, Math.Max(floorHeight, topZ - furnitureHeight), StateLastStep, 0);
                        break;

                    case FurnitureFootprint.Walkable:
                    case FurnitureFootprint.WalkableStopOnly:
                        // A platform is only solid at the deck you stand on. Furni data has no
                        // notion of an underside — stack height is just where the top surface
                        // sits — so treating the whole column as filled would wall off the ground
                        // beneath anything tall. An 8-high platform is a structure with 8 of clear
                        // space under it, not an 8-high brick: someone walks on top, someone else
                        // walks underneath.
                        AddSolid(ownerId, Math.Max(baseZ, topZ - PlatformThickness), topZ, false);
                        AddSurface(ownerId, topZ, footprint == FurnitureFootprint.Walkable ? StateOpen : StateLastStep, 0);
                        break;

                    default:
                        AddSolid(ownerId, baseZ, topZ, true);
                        break;
                }
            }

            public void AddSurface(int ownerId, double height, byte state, int gateGroupId)
            {
                if (state == StateBlocked)
                    return;

                _surfaces.Add(new Surface(ownerId, height, state, gateGroupId));

                if (gateGroupId != 0)
                    _gateGroups.Add(gateGroupId);
            }

            public void CommitTile(int x, int y, bool isDoorTile)
            {
                int tile = (y * _map.Width) + x;
                int baseIdx = tile * MaxLevels;
                int written = 0;

                _surfaces.Sort(static (a, b) => a.Height.CompareTo(b.Height));

                for (int i = 0; i < _surfaces.Count && written < MaxLevels; i++) {
                    Surface surface = _surfaces[i];

                    if (!HasHeadroom(surface))
                        continue;

                    if (written > 0 && surface.Height <= _map._surface[baseIdx + written - 1] + Epsilon) {
                        int previous = baseIdx + written - 1;

                        if (Restrictiveness(surface.State) > Restrictiveness(_map._state[previous]))
                            _map._state[previous] = surface.State;

                        if (surface.GateGroupId != 0)
                            _map._gateGroup[previous] = surface.GateGroupId;

                        continue;
                    }

                    _map._surface[baseIdx + written] = surface.Height;
                    _map._state[baseIdx + written] = isDoorTile ? StateLastStep : surface.State;
                    _map._gateGroup[baseIdx + written] = surface.GateGroupId;
                    written++;
                }

                _map._levelCount[tile] = (byte)written;

                _solids.Clear();
                _surfaces.Clear();
            }

            public RoomHeightMap Build()
            {
                if (_gateGroups.Count > 0) {
                    _map.GateGroupIds = new int[_gateGroups.Count];
                    _gateGroups.CopyTo(_map.GateGroupIds);
                }

                // One pass over a byte per tile, once per rebuild, so every later search knows
                // whether this room needs a 3D search at all.
                byte[] counts = _map._levelCount;
                int depth = 1;

                for (int i = 0; i < counts.Length; i++) {
                    if (counts[i] > depth)
                        depth = counts[i];
                }

                _map.LevelDepth = depth;

                return _map;
            }

            private bool HasHeadroom(Surface surface)
            {
                for (int i = 0; i < _solids.Count; i++) {
                    Solid solid = _solids[i];

                    if (solid.OwnerId == surface.OwnerId)
                        continue; // you never stand underneath yourself

                    if (solid.Top <= surface.Height + Epsilon)
                        continue; // entirely below the surface

                    if (solid.Bottom >= surface.Height + RequiredHeadroom - Epsilon)
                        continue; // clears the user's head

                    return false;
                }

                return true;
            }

            private static int Restrictiveness(byte state) => state switch
            {
                StateLastStep => 2,
                StateSeat => 1,
                _ => 0
            };

            private readonly struct Solid
            {
                public readonly int OwnerId;
                public readonly double Bottom;
                public readonly double Top;

                public Solid(int ownerId, double bottom, double top)
                {
                    OwnerId = ownerId;
                    Bottom = bottom;
                    Top = top;
                }
            }

            private readonly struct Surface
            {
                public readonly int OwnerId;
                public readonly double Height;
                public readonly byte State;
                public readonly int GateGroupId;

                public Surface(int ownerId, double height, byte state, int gateGroupId)
                {
                    OwnerId = ownerId;
                    Height = height;
                    State = state;
                    GateGroupId = gateGroupId;
                }
            }
        }
    }
}