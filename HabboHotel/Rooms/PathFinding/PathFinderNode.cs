using System;

namespace Plus.HabboHotel.Rooms.PathFinding
{
    public sealed class PathFinderNode : IComparable<PathFinderNode>
    {
        public readonly Vector2D Position;
        public PathFinderNode Next;
        public int Cost = int.MaxValue;
        public bool InOpen = false;
        public bool InClosed = false;

        public PathFinderNode(Vector2D position)
        {
            Position = position;
        }

        public override bool Equals(object obj)
        {
            return (obj is PathFinderNode) && ((PathFinderNode)obj).Position.Equals(Position);
        }

        public bool Equals(PathFinderNode breadcrumb)
        {
            return breadcrumb.Position.Equals(Position);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }

        public int CompareTo(PathFinderNode other)
        {
            return Cost.CompareTo(other.Cost);
        }
    }
}