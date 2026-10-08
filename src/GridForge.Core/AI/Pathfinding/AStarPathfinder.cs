using System;
using System.Collections.Generic;
using GridForge.Core.DataStructures;

namespace GridForge.Core.AI.Pathfinding
{
    public class AStarPathfinder
    {
        private readonly PathNode[,] _grid;
        private readonly int _width;
        private readonly int _height;

        public AStarPathfinder(PathNode[,] grid)
        {
            _grid = grid;
            _width = grid.GetLength(0);
            _height = grid.GetLength(1);
        }

        public List<PathNode> FindPath(int startX, int startY, int targetX, int targetY)
        {
            PathNode startNode = _grid[startX, startY];
            PathNode targetNode = _grid[targetX, targetY];

            if (!targetNode.IsWalkable) return new List<PathNode>();

            MinHeap<PathNode> openSet = new();
            HashSet<PathNode> closedSet = new();

            startNode.GCost = 0;
            startNode.HCost = CalculateHeuristic(startNode, targetNode);
            openSet.Enqueue(startNode);

            while (openSet.Count > 0)
            {
                PathNode current = openSet.Dequeue();

                if (current == targetNode)
                    return ReconstructPath(startNode, targetNode);

                closedSet.Add(current);

                foreach (PathNode neighbor in GetNeighbors(current))
                {
                    if (!neighbor.IsWalkable || closedSet.Contains(neighbor))
                        continue;

                    float newMovementCost = current.GCost + GetDistance(current, neighbor);
                    if (newMovementCost < neighbor.GCost || neighbor.GCost == 0)
                    {
                        neighbor.GCost = newMovementCost;
                        neighbor.HCost = CalculateHeuristic(neighbor, targetNode);
                        neighbor.Parent = current;

                        openSet.Enqueue(neighbor);
                    }
                }
            }

            return new List<PathNode>();
        }

        private List<PathNode> ReconstructPath(PathNode startNode, PathNode endNode)
        {
            List<PathNode> path = new();
            PathNode current = endNode;

            while (current != startNode)
            {
                path.Add(current);
                current = current.Parent;
            }

            path.Reverse();
            return path;
        }

        private float CalculateHeuristic(PathNode a, PathNode b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private float GetDistance(PathNode a, PathNode b)
        {
            return (a.X != b.X && a.Y != b.Y) ? 1.414f : 1.0f;
        }

        private List<PathNode> GetNeighbors(PathNode node)
        {
            List<PathNode> neighbors = new();

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int checkX = node.X + dx;
                    int checkY = node.Y + dy;

                    if (checkX >= 0 && checkX < _width && checkY >= 0 && checkY < _height)
                    {
                        neighbors.Add(_grid[checkX, checkY]);
                    }
                }
            }

            return neighbors;
        }
    }
}