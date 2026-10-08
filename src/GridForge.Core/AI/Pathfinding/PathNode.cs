using System;

namespace GridForge.Core.AI.Pathfinding
{
    public class PathNode : IComparable<PathNode>
    {
        public int X { get; }
        public int Y { get; }
        public bool IsWalkable { get; set; }

        public float GCost { get; set; } 
        public float HCost { get; set; } 
        public float FCost => GCost + HCost;

        public PathNode? Parent { get; set; }

        public PathNode(int x, int y, bool isWalkable = true)
        {
            X = x;
            Y = y;
            IsWalkable = isWalkable;
        }

        public int CompareTo(PathNode? other)
        {
            if (other == null) return 1;
            int compare = FCost.CompareTo(other.FCost);
            if (compare == 0)
            {
                compare = HCost.CompareTo(other.HCost);
            }
            return compare;
        }
    }
}