namespace GridForge.Core.Physics
{
    public struct Rectangle
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public Rectangle(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool Contains(Rectangle range)
        {
            return range.X >= X &&
                   range.X + range.Width <= X + Width &&
                   range.Y >= Y &&
                   range.Y + range.Height <= Y + Height;
        }

        public bool Intersects(Rectangle range)
        {
            return !(range.X > X + Width ||
                   range.X + range.Width < X ||
                   range.Y > Y + Height ||
                   range.Y + range.Height < Y);
        }
    }
}