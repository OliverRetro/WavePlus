using System;
using System.Collections.Generic;

namespace Plus.HabboHotel.Rooms.PathFinding
{
    /// <summary>
    /// 3D A* over a <see cref="RoomHeightMap"/> snapshot.
    ///
    /// Four things set it apart from the flat pathfinder it replaces:
    ///   * every tile can carry several standable surfaces, so a route may duck under furniture,
    ///     climb onto it, or drop off it inside one continuous path — in rooms that ask for it.
    ///     <see cref="PathContext.ThreeDEnabled"/> is off by default and collapses each tile to its
    ///     top surface, which is the classic behaviour and a search a quarter of the size;
    ///   * among equally short routes it always spends its diagonals first and walks the straight
    ///     remainder afterwards;
    ///   * it reads nothing but the snapshot it is handed, so it has no room locks to wait on and
    ///     can be run straight through on the room thread the moment a click lands;
    ///   * an unobstructed route never reaches the search at all — see <see cref="TryWalkTheLine"/>.
    ///
    /// Cost, measured in node expansions: a clear route across a 23x23 room is the length of the
    /// route (roughly 20), and the search only wakes up when something is actually in the way.
    /// That is what makes it affordable to re-plan the whole route synchronously on every click
    /// instead of handing out a guessed first step and searching for the rest in the background.
    /// </summary>
    public static class PathFinder
    {
        // Neighbour offsets. Order only matters for tie-breaking stability.
        private static readonly int[] DiagDx = { -1, 0, 1, 1, 1, 0, -1, -1 };
        private static readonly int[] DiagDy = { -1, -1, -1, 0, 1, 1, 1, 0 };
        private static readonly int[] CardDx = { 0, 1, 0, -1 };
        private static readonly int[] CardDy = { -1, 0, 1, 0 };

        // Costs are scaled by 1000 so the tie-breakers below can nudge the search between equally
        // short routes without ever competing with real distance.
        private const int CostStraight = 1000;
        private const int CostDiagonal = 1400;

        /// <summary>Charged for a straight step taken while a diagonal one would still have made
        /// progress on both axes. Same total distance, so this alone is what makes a route burn
        /// through its diagonals first and finish on the straight run.</summary>
        private const int DeferredDiagonalPenalty = 1;

        /// <summary>Charged for changing height, so flat ground wins ties against clambering.</summary>
        private const int LevelChangePenalty = 2;

        private const int MaxLineSteps = 128;

        private const int CoordBits = 9;
        private const int CoordMask = (1 << CoordBits) - 1;
        private const int LevelShift = CoordBits * 2;

        private const int HeapNodeBits = 22;
        private const int HeapNodeMask = (1 << HeapNodeBits) - 1;
        private const int HeapTieShift = HeapNodeBits;
        private const int HeapCostShift = HeapNodeBits + 8;

        [ThreadStatic] private static int[] _gCost;
        [ThreadStatic] private static int[] _parent;
        [ThreadStatic] private static int[] _stamp;
        [ThreadStatic] private static bool[] _closed;
        [ThreadStatic] private static int[] _passStamp;
        [ThreadStatic] private static bool[] _passable;
        [ThreadStatic] private static int _generation;
        [ThreadStatic] private static BinaryHeap _open;
        [ThreadStatic] private static LineStep[] _line;

        /// <summary>
        /// Full route from a start position to a goal tile, ordered [goal, ..., start] to match the
        /// walk loop's indexing. Empty when there is no route.
        /// </summary>
        public static List<Vector3D> FindPath(PathContext ctx, int startX, int startY, double startZ, int goalX, int goalY)
        {
            var path = new List<Vector3D>();

            RoomHeightMap map = ctx?.HeightMap;
            if (map == null)
                return path;

            if (!map.InBounds(startX, startY) || !map.InBounds(goalX, goalY))
                return path;

            if (startX == goalX && startY == goalY)
                return path;

            int startTile = map.TileIndex(startX, startY);
            int goalTile = map.TileIndex(goalX, goalY);

            // Clicking a table, a wall block, anything you can't stand on: walk up to it instead of
            // refusing to move. The search then accepts any tile touching the target, and the
            // heuristic still aims at the target itself so it approaches from the near side.
            bool approachOnly = !ctx.HasEnterableLevel(goalTile);

            if (approachOnly && IsAdjacent(startX, startY, goalX, goalY))
                return path; // already standing next to it

            // A user can legitimately be standing where the map has no surface at all — teleported
            // onto a blocked square, or furniture dropped on their head. Path out of it anyway.
            int startLevel = map.FindLevel(startX, startY, startZ);
            double startSurface = startLevel >= 0 ? map.SurfaceAt(startTile, startLevel) : startZ;

            // The overwhelmingly common click: nothing between the avatar and the tile it was aimed
            // at. Answering that without a search is the whole reason a re-plan can be handed out
            // on the same tick the click arrives.
            if (!approachOnly && ctx.DiagonalEnabled &&
                TryWalkTheLine(ctx, map, startX, startY, startSurface, goalX, goalY, path))
                return path;

            return Search(ctx, map, path, startX, startY, startTile, startSurface, goalX, goalY, goalTile, approachOnly);
        }

        private static bool TryWalkTheLine(PathContext ctx, RoomHeightMap map, int startX, int startY, double startSurface, int goalX, int goalY, List<Vector3D> path)
        {
            int dx = goalX - startX;
            int dy = goalY - startY;

            int absX = Math.Abs(dx);
            int absY = Math.Abs(dy);

            int steps = Math.Max(absX, absY);
            if (steps > MaxLineSteps)
                return false;

            int diagonals = Math.Min(absX, absY);
            int stepX = Math.Sign(dx);
            int stepY = Math.Sign(dy);

            // After the diagonals are spent the rest of the run is along whichever axis still has
            // ground to cover.
            int tailX = absX > absY ? stepX : 0;
            int tailY = absX > absY ? 0 : stepY;

            LineStep[] line = _line ??= new LineStep[MaxLineSteps];

            int x = startX;
            int y = startY;
            double surface = startSurface;

            for (int i = 0; i < steps; i++) {
                bool diagonal = i < diagonals;

                x += diagonal ? stepX : tailX;
                y += diagonal ? stepY : tailY;

                int tile = map.TileIndex(x, y);
                bool isFinalStep = i == steps - 1;

                // SelectLevel applies the same entry and climb/fall rules the search does, so a
                // line that clears it is walkable by exactly the same definition.
                int level = ctx.SelectLevel(tile, surface, isFinalStep);
                if (level < 0)
                    return false;

                surface = ctx.SurfaceOf(tile, level);

                line[i] = new LineStep(x, y, surface);
            }

            // Legacy order: [goal, ..., start].
            for (int i = steps - 1; i >= 0; i--) {
                LineStep step = line[i];
                path.Add(new Vector3D(step.X, step.Y, step.Z));
            }

            path.Add(new Vector3D(startX, startY, startSurface));

            return true;
        }

        private static List<Vector3D> Search(PathContext ctx, RoomHeightMap map, List<Vector3D> path, int startX, int startY, int startTile, double startSurface, int goalX, int goalY, int goalTile, bool approachOnly)
        {
            int width = map.Width;
            int height = map.Height;
            int size = width * height;

            // Only the levels this search can actually branch into: the levels the room stacks when
            // it pathfinds in 3D, and exactly one when it does not. Either way a room with nothing
            // to walk under is a plain 2D grid here — node == tile, and the search touches a
            // quarter of the memory the fixed four-level layout would have made it stamp, close
            // and probe.
            int depth = ctx.SearchDepth;
            int nodeCount = size * depth;

            EnsureScratch(nodeCount);

            int generation = NextGeneration();

            int startLevel = Math.Min(Math.Max(map.FindLevel(startX, startY, startSurface), 0), depth - 1);
            int startNode = (startLevel * size) + startTile;
            int startPacked = Pack(startX, startY, startLevel);

            bool diagonal = ctx.DiagonalEnabled;
            int[] dxs = diagonal ? DiagDx : CardDx;
            int[] dys = diagonal ? DiagDy : CardDy;
            int neighbours = dxs.Length;

            int closestPacked = -1;
            int closestHeuristic = Heuristic(startX, startY, goalX, goalY, diagonal);
            int closestCost = 0;

            BinaryHeap open = _open;
            open.Clear();

            _gCost[startNode] = 0;
            _parent[startNode] = -1;
            _stamp[startNode] = generation;
            _closed[startNode] = false;
            open.Push(0, closestHeuristic, startPacked);

            while (open.Count > 0) {
                int current = open.Pop();

                int cx = current & CoordMask;
                int cy = (current >> CoordBits) & CoordMask;
                int clevel = current >> LevelShift;

                int currentTile = (cy * width) + cx;
                int currentNode = (clevel * size) + currentTile;

                if (_closed[currentNode])
                    continue; // stale duplicate heap entry

                if (currentTile == goalTile)
                    return Reconstruct(ctx, map, path, current, width, startPacked, startSurface);

                _closed[currentNode] = true;

                // Cheapest tile touching an unreachable target — that is as close as we get.
                if (approachOnly && currentNode != startNode && IsAdjacent(cx, cy, goalX, goalY))
                    return Reconstruct(ctx, map, path, current, width, startPacked, startSurface);

                int currentCost = _gCost[currentNode];

                if (currentNode != startNode) {
                    int heuristic = Heuristic(cx, cy, goalX, goalY, diagonal);

                    if (heuristic < closestHeuristic || (heuristic == closestHeuristic && closestPacked != -1 && currentCost < closestCost)) {
                        closestHeuristic = heuristic;
                        closestCost = currentCost;
                        closestPacked = current;
                    }
                }

                double fromSurface = currentNode == startNode ? startSurface : ctx.SurfaceOf(currentTile, clevel);

                // Diagonals only "make progress on both axes" while the goal is off-axis; once it
                // lines up, straight steps are free again.
                bool diagonalStillUseful = diagonal && cx != goalX && cy != goalY;

                for (int i = 0; i < neighbours; i++) {
                    int nx = cx + dxs[i];
                    int ny = cy + dys[i];

                    if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                        continue;

                    int neighbourTile = (ny * width) + nx;

                    int levels = ctx.UsableLevelCount(neighbourTile);
                    if (levels == 0)
                        continue; // nothing to stand on, whatever height you arrive at

                    bool isFinalStep = neighbourTile == goalTile;
                    bool isDiagonalStep = nx != cx && ny != cy;

                    int stepCost = isDiagonalStep ? CostDiagonal : CostStraight;
                    if (!isDiagonalStep && diagonalStillUseful)
                        stepCost += DeferredDiagonalPenalty;

                    int heuristic = Heuristic(nx, ny, goalX, goalY, diagonal);

                    for (int level = 0; level < levels; level++) {
                        int neighbourNode = (level * size) + neighbourTile;
                        bool seen = _stamp[neighbourNode] == generation;

                        if (seen && _closed[neighbourNode])
                            continue;

                        double toSurface = ctx.SurfaceOf(neighbourTile, level);

                        int cost = stepCost;
                        if (Math.Abs(toSurface - fromSurface) > RoomHeightMap.Epsilon) {
                            if (!ctx.AllowOverride && !ctx.IgnoreHeightLimits && !RoomHeightMap.CanTransition(fromSurface, toSurface))
                                continue;

                            cost += LevelChangePenalty;
                        }

                        int tentative = currentCost + cost;

                        if (seen && tentative >= _gCost[neighbourNode])
                            continue;

                        // Left until last: it is the only test that has to look past the height map
                        // into gates and live occupancy, and by here most neighbours have already
                        // been rejected on cost alone. Within one search the goal tile is only ever
                        // reached as a final step, so the answer never depends on which side we
                        // arrive from and it is safe to remember it.
                        if (!Passable(ctx, neighbourNode, neighbourTile, level, isFinalStep, generation))
                            continue;

                        _stamp[neighbourNode] = generation;
                        _closed[neighbourNode] = false;
                        _gCost[neighbourNode] = tentative;
                        _parent[neighbourNode] = current;

                        open.Push(tentative, heuristic, Pack(nx, ny, level));
                    }
                }
            }

            // walk to nearest if target is impossible
            if (closestPacked != -1)
                return Reconstruct(ctx, map, path, closestPacked, width, startPacked, startSurface);

            return path; // nothing reachable is any closer than where we already stand
        }

        private static int Pack(int x, int y, int level) => x | (y << CoordBits) | (level << LevelShift);

        /// <summary>
        /// <see cref="PathContext.CanEnter"/>, answered once per node per search.
        ///
        /// Neighbours are reached from up to eight directions and the answer cannot change while a
        /// search runs, so without this the gate and occupancy lookups are repeated several times
        /// over for every tile the search touches.
        /// </summary>
        private static bool Passable(PathContext ctx, int node, int tile, int level, bool isFinalStep, int generation)
        {
            if (_passStamp[node] == generation)
                return _passable[node];

            bool passable = ctx.CanEnter(tile, level, isFinalStep);

            _passStamp[node] = generation;
            _passable[node] = passable;

            return passable;
        }

        private static bool IsAdjacent(int x, int y, int otherX, int otherY)
        {
            return Math.Abs(x - otherX) <= 1 && Math.Abs(y - otherY) <= 1;
        }

        private static int Heuristic(int x, int y, int goalX, int goalY, bool diagonal)
        {
            int dx = Math.Abs(x - goalX);
            int dy = Math.Abs(y - goalY);

            if (!diagonal)
                return CostStraight * (dx + dy);

            // Octile distance — admissible for 8-directional movement. It ignores the tie-break
            // penalties above, which only ever make a real route more expensive, so it stays
            // admissible and A* still returns a shortest path.
            int min = Math.Min(dx, dy);
            int max = Math.Max(dx, dy);
            return (CostDiagonal * min) + (CostStraight * (max - min));
        }

        private static List<Vector3D> Reconstruct(PathContext ctx, RoomHeightMap map, List<Vector3D> path, int goalPacked, int width, int startPacked, double startSurface)
        {
            // Legacy order: [goal, ..., start].
            int packed = goalPacked;
            int size = width * map.Height;

            while (packed != -1) {
                int x = packed & CoordMask;
                int y = (packed >> CoordBits) & CoordMask;
                int level = packed >> LevelShift;

                int tile = (y * width) + x;
                double surface = packed == startPacked ? startSurface : ctx.SurfaceOf(tile, level);

                path.Add(new Vector3D(x, y, surface));

                packed = _parent[(level * size) + tile];
            }

            return path;
        }

        private static void EnsureScratch(int nodeCount)
        {
            if (_gCost != null && _gCost.Length >= nodeCount) {
                return;
            }

            // Rooms only ever grow, and a handful of rooms per thread share these buffers, so
            // rounding up keeps re-allocation to a one-off.
            int capacity = Math.Max(nodeCount, 4096);

            _gCost = new int[capacity];
            _parent = new int[capacity];
            _stamp = new int[capacity];
            _closed = new bool[capacity];
            _passStamp = new int[capacity];
            _passable = new bool[capacity];
            _generation = 0;
            _open ??= new BinaryHeap(1024);
        }

        /// <summary>Stamping beats clearing the scratch arrays per search; the only cost is one
        /// wrap check per call.</summary>
        private static int NextGeneration()
        {
            if (_generation == int.MaxValue) {
                Array.Clear(_stamp, 0, _stamp.Length);
                Array.Clear(_passStamp, 0, _passStamp.Length);
                _generation = 0;
            }

            return ++_generation;
        }

        /// <summary>One tile of a straight-line probe, held in a reusable buffer so a probe that
        /// fails costs nothing at all.</summary>
        private readonly struct LineStep
        {
            public readonly int X;
            public readonly int Y;
            public readonly double Z;

            public LineStep(int x, int y, double z)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }

        /// <summary>
        /// Binary heap of (priority, node) pairs held as a single long each — priority in the high
        /// half, the packed node in the low half.
        ///
        /// One array instead of two is the whole point: an obstructed search sifts tens of
        /// thousands of entries, and the parallel-array version touched two cache lines and did two
        /// swaps for every one of those. Comparing the combined long orders by priority first and
        /// then by node, which is a stable tie-break rather than an arbitrary one.
        /// </summary>
        private sealed class BinaryHeap
        {
            private long[] _entries;

            public int Count { get; private set; }

            public BinaryHeap(int capacity)
            {
                _entries = new long[capacity];
            }

            public void Clear()
            {
                Count = 0;
            }

            public void Push(int cost, int heuristic, int node)
            {
                long[] entries = _entries;

                if (Count == entries.Length) {
                    Array.Resize(ref _entries, Count * 2);
                    entries = _entries;
                }

                int tie = heuristic >> 10; // ~one unit per tile still to cover
                if (tie > 255)
                    tie = 255;

                long entry = ((long)(cost + heuristic) << HeapCostShift) | ((long)tie << HeapTieShift) | (uint)node;

                int i = Count++;

                while (i > 0) {
                    int p = (i - 1) >> 1;
                    long parent = entries[p];

                    if (parent <= entry)
                        break;

                    entries[i] = parent;
                    i = p;
                }

                entries[i] = entry;
            }

            public int Pop()
            {
                long[] entries = _entries;

                long result = entries[0];

                int count = --Count;
                long last = entries[count];

                int i = 0;
                while (true) {
                    int l = (i << 1) + 1;
                    if (l >= count)
                        break;

                    int r = l + 1;
                    int smallest = (r < count && entries[r] < entries[l]) ? r : l;

                    if (last <= entries[smallest])
                        break;

                    entries[i] = entries[smallest];
                    i = smallest;
                }

                if (count > 0)
                    entries[i] = last;

                return (int)result & HeapNodeMask;
            }
        }
    }
}