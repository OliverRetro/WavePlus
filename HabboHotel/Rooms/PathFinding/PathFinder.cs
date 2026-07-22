using System;
using System.Collections.Generic;

namespace Plus.HabboHotel.Rooms.PathFinding
{
    /// <summary>
    /// A* pathfinder over the room's tile grid.
    ///
    /// Rewritten from the legacy implementation which (a) folded a squared-euclidean
    /// heuristic into the accumulated g-cost — making the search wildly inadmissible and
    /// producing wrong/failed paths around obstacles — and (b) mutated node costs after
    /// they were already inside the heap, silently corrupting the heap invariant. This
    /// version keeps g and h separate, uses flat arrays instead of per-node allocations,
    /// and tolerates stale heap entries by skipping already-closed pops.
    /// </summary>
    public static class PathFinder
    {
        // Neighbour offsets. Order matters for tie-breaking stability only.
        private static readonly int[] DiagDx = { -1, 0, 1, 1, 1, 0, -1, -1 };
        private static readonly int[] DiagDy = { -1, -1, -1, 0, 1, 1, 1, 0 };
        private static readonly int[] CardDx = { 0, 1, 0, -1 };
        private static readonly int[] CardDy = { -1, 0, 1, 0 };

        private const int CostStraight = 10;
        private const int CostDiagonal = 14;

        /// <summary>
        /// Returns the path as [end, ..., firstStep, start] (legacy order — consumers walk
        /// it back-to-front), or an empty list when no path exists.
        /// </summary>
        public static List<Vector2D> FindPath(RoomUser user, bool diag, Gamemap map, Vector2D start, Vector2D end)
        {
            var path = new List<Vector2D>();

            if (map?.Model == null || start == null || end == null)
                return path;

            int width = map.Model.MapSizeX;
            int height = map.Model.MapSizeY;

            if (width <= 0 || height <= 0)
                return path;

            if (start.X < 0 || start.Y < 0 || start.X >= width || start.Y >= height)
                return path;
            if (end.X < 0 || end.Y < 0 || end.X >= width || end.Y >= height)
                return path;

            int size = width * height;
            int startIdx = (start.Y * width) + start.X;
            int endIdx = (end.Y * width) + end.X;

            if (startIdx == endIdx)
                return path;

            bool allowOverride = user != null && user.AllowOverride;

            // gCost == int.MaxValue means "not seen yet"; closed is tracked separately.
            int[] gCost = new int[size];
            int[] parent = new int[size];
            bool[] closed = new bool[size];
            for (int i = 0; i < size; i++) {
                gCost[i] = int.MaxValue;
                parent[i] = -1;
            }

            var open = new BinaryHeap(64);

            gCost[startIdx] = 0;
            open.Push(Heuristic(start.X, start.Y, end.X, end.Y, diag), startIdx);

            int[] dxs = diag ? DiagDx : CardDx;
            int[] dys = diag ? DiagDy : CardDy;
            int neighbourCount = dxs.Length;

            // Reused Vector2D instances for the step-validity call to avoid two allocations
            // per neighbour probe.
            var fromVec = new Vector2D();
            var toVec = new Vector2D();

            while (open.Count > 0) {
                int currentIdx = open.Pop();

                if (closed[currentIdx])
                    continue; // stale duplicate entry

                if (currentIdx == endIdx)
                    return Reconstruct(parent, endIdx, width);

                closed[currentIdx] = true;

                int cx = currentIdx % width;
                int cy = currentIdx / width;

                for (int i = 0; i < neighbourCount; i++) {
                    int nx = cx + dxs[i];
                    int ny = cy + dys[i];

                    if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                        continue;

                    int nIdx = (ny * width) + nx;
                    if (closed[nIdx])
                        continue;

                    bool isFinalMove = nIdx == endIdx;

                    fromVec.X = cx;
                    fromVec.Y = cy;
                    toVec.X = nx;
                    toVec.Y = ny;

                    if (!map.IsValidStep(fromVec, toVec, isFinalMove, allowOverride))
                        continue;

                    bool diagonalStep = nx != cx && ny != cy;
                    int tentativeG = gCost[currentIdx] + (diagonalStep ? CostDiagonal : CostStraight);

                    if (tentativeG >= gCost[nIdx])
                        continue;

                    gCost[nIdx] = tentativeG;
                    parent[nIdx] = currentIdx;
                    open.Push(tentativeG + Heuristic(nx, ny, end.X, end.Y, diag), nIdx);
                }
            }

            return path; // no path
        }

        private static int Heuristic(int x, int y, int endX, int endY, bool diag)
        {
            int dx = Math.Abs(x - endX);
            int dy = Math.Abs(y - endY);

            if (!diag)
                return CostStraight * (dx + dy);

            // Octile distance — admissible for 8-directional movement.
            int min = Math.Min(dx, dy);
            int max = Math.Max(dx, dy);
            return (CostDiagonal * min) + (CostStraight * (max - min));
        }

        private static List<Vector2D> Reconstruct(int[] parent, int endIdx, int width)
        {
            // Legacy order: [end, ..., start].
            var path = new List<Vector2D>();
            int idx = endIdx;
            while (idx != -1) {
                path.Add(new Vector2D(idx % width, idx / width));
                idx = parent[idx];
            }

            return path;
        }

        /// <summary>Minimal binary min-heap of (priority, tile index) pairs.</summary>
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