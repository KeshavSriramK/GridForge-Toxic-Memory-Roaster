using System.Collections.Generic;

namespace GridForge.Core.Physics
{
    public interface ICollidable
    {
        Rectangle BoundingBox { get; }
    }

    public class QuadTree
    {
        private const int Capacity = 4;
        private int _level;
        private Rectangle _boundary;
        private List<ICollidable> _objects;
        private bool _isDivided;

        private QuadTree[] _nodes;

        public QuadTree(int level, Rectangle boundary)
        {
            _level = level;
            _boundary = boundary;
            _objects = new List<ICollidable>();
            _isDivided = false;
            _nodes = new QuadTree[4];
        }

        public void Clear()
        {
            _objects.Clear();
            _isDivided = false;

            for (int i = 0; i < _nodes.Length; i++)
            {
                if (_nodes[i] != null)
                {
                    _nodes[i].Clear();
                    _nodes[i] = null!;
                }
            }
        }

        private void Subdivide()
        {
            float subWidth = _boundary.Width / 2f;
            float subHeight = _boundary.Height / 2f;
            float x = _boundary.X;
            float y = _boundary.Y;

            _nodes[0] = new QuadTree(_level + 1, new Rectangle(x + subWidth, y, subWidth, subHeight));
            _nodes[1] = new QuadTree(_level + 1, new Rectangle(x, y, subWidth, subHeight));
            _nodes[2] = new QuadTree(_level + 1, new Rectangle(x, y + subHeight, subWidth, subHeight));
            _nodes[3] = new QuadTree(_level + 1, new Rectangle(x + subWidth, y + subHeight, subWidth, subHeight));

            _isDivided = true;
        }

        public bool Insert(ICollidable item)
        {
            if (!_boundary.Contains(item.BoundingBox))
                return false;

            if (_objects.Count < Capacity && !_isDivided)
            {
                _objects.Add(item);
                return true;
            }

            if (!_isDivided)
            {
                Subdivide();
            }

            foreach (var node in _nodes)
            {
                if (node.Insert(item))
                    return true;
            }

            _objects.Add(item);
            return true;
        }

        public List<ICollidable> Query(Rectangle range, List<ICollidable> found)
        {
            if (!_boundary.Intersects(range))
                return found;

            foreach (var obj in _objects)
            {
                if (range.Intersects(obj.BoundingBox))
                    found.Add(obj);
            }

            if (_isDivided)
            {
                for (int i = 0; i < _nodes.Length; i++)
                {
                    _nodes[i].Query(range, found);
                }
            }

            return found;
        }
    }
}