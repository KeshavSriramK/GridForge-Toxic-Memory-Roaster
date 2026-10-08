using Xunit;
using GridForge.Core.Physics;
using System.Collections.Generic;

namespace GridForge.Tests
{
    public class TestEntity : ICollidable
    {
        public Rectangle BoundingBox { get; }
        public TestEntity(float x, float y, float width, float height)
        {
            BoundingBox = new Rectangle(x, y, width, height);
        }
    }

    public class QuadTreeTests
    {
        [Fact]
        public void QuadTree_QueriesEntitiesCorrectly()
        {
            var bounds = new Rectangle(0, 0, 100, 100);
            var quadTree = new QuadTree(0, bounds);

            var entity1 = new TestEntity(10, 10, 5, 5);
            var entity2 = new TestEntity(80, 80, 5, 5);

            quadTree.Insert(entity1);
            quadTree.Insert(entity2);

            var queryRange = new Rectangle(0, 0, 30, 30);
            var results = quadTree.Query(queryRange, new List<ICollidable>());

            Assert.Single(results);
            Assert.Contains(entity1, results);
            Assert.DoesNotContain(entity2, results);
        }
    }
}