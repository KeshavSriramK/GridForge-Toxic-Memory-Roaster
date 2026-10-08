using Xunit;
using GridForge.Core.AI.Pathfinding;
using System.Collections.Generic;

namespace GridForge.Tests
{
    public class PathfindingTests
    {
        [Fact]
        public void FindPath_FindsValidPathAroundObstacle()
        {
            // Set up a 5x5 grid
            PathNode[,] grid = new PathNode[5, 5];
            for (int x = 0; x < 5; x++)
                for (int y = 0; y < 5; y++)
                    grid[x, y] = new PathNode(x, y);

            // Block direct horizontal path with a vertical wall at x=1
            grid[1, 0].IsWalkable = false;
            grid[1, 1].IsWalkable = false;
            grid[1, 2].IsWalkable = false;

            var pathfinder = new AStarPathfinder(grid);
            List<PathNode> path = pathfinder.FindPath(0, 0, 2, 0);

            Assert.NotEmpty(path);
            Assert.Equal(2, path[path.Count - 1].X);
            Assert.Equal(0, path[path.Count - 1].Y);
        }
    }
}