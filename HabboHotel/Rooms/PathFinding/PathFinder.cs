using System;
using System.Collections.Generic;

namespace Plus.HabboHotel.Rooms.PathFinding
{
    /// <summary>
    /// 3D A* over a <see cref="RoomHeightMap"/> snapshot.
    ///
    /// Three things set it apart from the flat pathfinder it replaces:
    ///   * every tile can carry several standable surfaces, so a route may duck under furniture,
    ///     climb onto it, or drop off it inside one continuous path;
    ///   * among equally short routes it always spends its diagonals first and walks the straight
    ///     remainder afterwards;
    ///   * it reads nothing but the snapshot it is handed, so it is safe to run off the room thread
    ///     while the user is already walking their first step.
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

        [ThreadStatic] private static int[] _gCost;
        [ThreadStatic] private static int[] _parent;
        [ThreadStatic] private static int[] _stamp;
        [ThreadStatic] private static bool[] _closed;
        [ThreadStatic] private static int _generation;
        [ThreadStatic] private static BinaryHeap _open;

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

            int width = map.Width;
            int size = width * map.Height;
            int nodeCount = size * RoomHeightMap.MaxLevels;

            EnsureScratch(nodeCount);

            int generation = NextGeneration();

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
            int startNode = (Math.Max(startLevel, 0) * size) + startTile;

            bool diagonal = ctx.DiagonalEnabled;
            int[] dxs = diagonal ? DiagDx : CardDx;
            int[] dys = diagonal ? DiagDy : CardDy;
            int neighbours = dxs.Length;

            BinaryHeap open = _open;
            open.Clear();

            _gCost[startNode] = 0;
            _parent[startNode] = -1;
            _stamp[startNode] = generation;
            _closed[startNode] = false;
            open.Push(Heuristic(startX, startY, goalX, goalY, diagonal), startNode);

            while (open.Count > 0) {
                int current = open.Pop();

                if (_closed[current])
                    continue; // stale duplicate heap entry

                int currentTile = current % size;

                if (currentTile == goalTile)
                    return Reconstruct(map, current, size, width, startNode, startSurface);

                _closed[current] = true;

                int cx = currentTile % width;
                int cy = currentTile / width;

                // Cheapest tile touching an unreachable target — that is as close as we get.
                if (approachOnly && current != startNode && IsAdjacent(cx, cy, goalX, goalY))
                    return Reconstruct(map, current, size, width, startNode, startSurface);
                double fromSurface = current == startNode ? startSurface : map.SurfaceAt(currentTile, current / size);

                // Diagonals only "make progress on both axes" while the goal is off-axis; once it
                // lines up, straight steps are free again.
                bool diagonalStillUseful = diagonal && cx != goalX && cy != goalY;

                for (int i = 0; i < neighbours; i++) {
                    int nx = cx + dxs[i];
                    int ny = cy + dys[i];

                    if (nx < 0 || ny < 0 || nx >= width || ny >= map.Height)
                        continue;

                    int neighbourTile = (ny * width) + nx;
                    bool isFinalStep = neighbourTile == goalTile;
                    bool isDiagonalStep = nx != cx && ny != cy;

                    int stepCost = isDiagonalStep ? CostDiagonal : CostStraight;
                    if (!isDiagonalStep && diagonalStillUseful)
                        stepCost += DeferredDiagonalPenalty;

                    int levels = map.LevelCountAt(neighbourTile);

                    for (int level = 0; level < levels; level++) {
                        int neighbourNode = (level * size) + neighbourTile;

                        if (_stamp[neighbourNode] == generation && _closed[neighbourNode])
                            continue;

                        if (!ctx.CanEnter(neighbourTile, level, isFinalStep))
                            continue;

                        double toSurface = map.SurfaceAt(neighbourTile, level);

                        if (!ctx.AllowOverride && !ctx.IgnoreHeightLimits && !RoomHeightMap.CanTransition(fromSurface, toSurface))
                            continue;

                        int cost = stepCost;
                        if (Math.Abs(toSurface - fromSurface) > RoomHeightMap.Epsilon)
                            cost += LevelChangePenalty;

                        int tentative = _gCost[current] + cost;

                        if (_stamp[neighbourNode] == generation && tentative >= _gCost[neighbourNode])
                            continue;

                        _stamp[neighbourNode] = generation;
                        _closed[neighbourNode] = false;
                        _gCost[neighbourNode] = tentative;
                        _parent[neighbourNode] = current;

                        open.Push(tentative + Heuristic(nx, ny, goalX, goalY, diagonal), neighbourNode);
                    }
                }
            }

            return path; // unreachable
        }

        /// <summary>
        /// The single best step towards a goal, worked out from the eight neighbours alone.
        ///
        /// This is what gets a user moving on the very tick they clicked: the full route is only
        /// needed by the time that step lands, and a one-tile walk never needs one at all.
        /// Returns null when no neighbour makes progress, which is the caller's cue that the route
        /// has to go around something and a full search is required up front.
        /// </summary>
        public static Vector3D FindNextStep(PathContext ctx, int startX, int startY, double startZ, int goalX, int goalY)
        {
            RoomHeightMap map = ctx?.HeightMap;
            if (map == null)
                return null;

            if (!map.InBounds(startX, startY) || !map.InBounds(goalX, goalY))
                return null;

            if (startX == goalX && startY == goalY)
                return null;

            bool diagonal = ctx.DiagonalEnabled;
            int[] dxs = diagonal ? DiagDx : CardDx;
            int[] dys = diagonal ? DiagDy : CardDy;

            int startTile = map.TileIndex(startX, startY);
            int startLevel = map.FindLevel(startX, startY, startZ);
            double fromSurface = startLevel >= 0 ? map.SurfaceAt(startTile, startLevel) : startZ;

            int currentDistance = Heuristic(startX, startY, goalX, goalY, diagonal);
            bool diagonalStillUseful = diagonal && startX != goalX && startY != goalY;

            Vector3D best = null;
            int bestScore = int.MaxValue;

            for (int i = 0; i < dxs.Length; i++) {
                int nx = startX + dxs[i];
                int ny = startY + dys[i];

                if (!map.InBounds(nx, ny))
                    continue;

                int distance = Heuristic(nx, ny, goalX, goalY, diagonal);

                // Only ever commit to a step that genuinely closes the gap; anything else is a
                // detour, and detours are the full search's job.
                if (distance >= currentDistance)
                    continue;

                int neighbourTile = map.TileIndex(nx, ny);
                bool isFinalStep = nx == goalX && ny == goalY;

                int level = ctx.SelectLevel(neighbourTile, fromSurface, isFinalStep);
                if (level < 0)
                    continue;

                bool isDiagonalStep = nx != startX && ny != startY;
                int score = distance + (isDiagonalStep ? CostDiagonal : CostStraight);

                if (!isDiagonalStep && diagonalStillUseful)
                    score += DeferredDiagonalPenalty;

                double surface = map.SurfaceAt(neighbourTile, level);
                if (Math.Abs(surface - fromSurface) > RoomHeightMap.Epsilon)
                    score += LevelChangePenalty;

                if (score >= bestScore)
                    continue;

                bestScore = score;
                best = new Vector3D(nx, ny, surface);
            }

            return best;
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

        private static List<Vector3D> Reconstruct(RoomHeightMap map, int goalNode, int size, int width, int startNode, double startSurface)
        {
            // Legacy order: [goal, ..., start].
            var path = new List<Vector3D>();
            int node = goalNode;

            while (node != -1) {
                int tile = node % size;
                double surface = node == startNode ? startSurface : map.SurfaceAt(tile, node / size);

                path.Add(new Vector3D(tile % width, tile / width, surface));

                node = _parent[node];
            }

            return path;
        }

        private static void EnsureScratch(int nodeCount)
        {
            if (_gCost != null && _gCost.Length >= nodeCount)
                return;

            // Rooms only ever grow, and a handful of rooms per thread share these buffers, so
            // rounding up keeps re-allocation to a one-off.
            int capacity = Math.Max(nodeCount, 4096);

            _gCost = new int[capacity];
            _parent = new int[capacity];
            _stamp = new int[capacity];
            _closed = new bool[capacity];
            _generation = 0;
            _open ??= new BinaryHeap(256);
        }

        /// <summary>Stamping beats clearing three arrays per search; the only cost is one wrap
        /// check per call.</summary>
        private static int NextGeneration()
        {
            if (_generation == int.MaxValue) {
                Array.Clear(_stamp, 0, _stamp.Length);
                _generation = 0;
            }

            return ++_generation;
        }

        private sealed class BinaryHeap
        {
            private int[] _priority;
            private int[] _value;

            public int Count { get; private set; }

            public BinaryHeap(int capacity)
            {
                _priority = new int[capacity];
                _value = new int[capacity];
            }

            public void Clear()
            {
                Count = 0;
            }

            public void Push(int priority, int value)
            {
                if (Count == _priority.Length) {
                    Array.Resize(ref _priority, Count * 2);
                    Array.Resize(ref _value, Count * 2);
                }

                int i = Count++;
                _priority[i] = priority;
                _value[i] = value;

                while (i > 0) {
                    int p = (i - 1) >> 1;
                    if (_priority[p] <= _priority[i])
                        break;

                    (_priority[p], _priority[i]) = (_priority[i], _priority[p]);
                    (_value[p], _value[i]) = (_value[i], _value[p]);
                    i = p;
                }
            }

            public int Pop()
            {
                int result = _value[0];
                Count--;
                _priority[0] = _priority[Count];
                _value[0] = _value[Count];

                int i = 0;
                while (true) {
                    int l = (i << 1) + 1;
                    if (l >= Count)
                        break;

                    int r = l + 1;
                    int smallest = (r < Count && _priority[r] < _priority[l]) ? r : l;

                    if (_priority[i] <= _priority[smallest])
                        break;

                    (_priority[i], _priority[smallest]) = (_priority[smallest], _priority[i]);
                    (_value[i], _value[smallest]) = (_value[smallest], _value[i]);
                    i = smallest;
                }

                return result;
            }
        }
    }
}
