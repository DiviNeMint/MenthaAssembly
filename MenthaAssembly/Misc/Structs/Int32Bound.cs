using System;

namespace MenthaAssembly
{
    public struct Int32Bound
    {
        public static Int32Bound Empty => new Int32Bound();

        public int Left { set; get; }

        public int Top { set; get; }

        public int Right { set; get; }

        public int Bottom { set; get; }

        public int Width => Right - Left;

        public int Height => Bottom - Top;

        public bool IsEmpty
            => Width is 0 || Height is 0;

        public Int32Bound(int Left, int Top, int Right, int Bottom)
        {
            this.Left = Left;
            this.Top = Top;
            this.Right = Right;
            this.Bottom = Bottom;
        }

        public Int32Bound(double Left, double Top, double Right, double Bottom)
        {
            this.Left = (int)Left;
            this.Top = (int)Top;
            this.Right = (int)Right;
            this.Bottom = (int)Bottom;
        }

        public Int32Bound(Int32Point Position, Int32Size Size)
        {
            this.Left = Position.X;
            this.Top = Position.Y;
            this.Right = Position.X + Size.Width;
            this.Bottom = Position.Y + Size.Height;
        }

        public void Intersect(Int32Bound Bound)
        {
            if (this.IntersectsWith(Bound))
            {
                Left = Math.Max(Left, Bound.Left);
                Right = Math.Max(Math.Min(Right, Bound.Right), Left);
                Top = Math.Max(Top, Bound.Top);
                Bottom = Math.Max(Math.Min(Bottom, Bound.Bottom), Top);
            }
            else
            {
                this = Empty;
            }
        }
        public bool IntersectsWith(Int32Bound Bound)
        {
            if (IsEmpty || Bound.IsEmpty)
                return false;

            return Bound.Left <= Right &&
                   Bound.Right >= Left &&
                   Bound.Top <= Bottom &&
                   Bound.Bottom >= Top;
        }

        public void Union(Int32Bound Bound)
        {
            if (IsEmpty)
            {
                this = Bound;
                return;
            }

            if (Bound.IsEmpty)
                return;

            Left = Math.Min(Left, Bound.Left);
            Right = Math.Max(Right, Bound.Right);
            Top = Math.Min(Top, Bound.Top);
            Bottom = Math.Max(Bottom, Bound.Bottom);
        }

        public void Offset(Int32Vector Vector)
            => Offset(Vector.X, Vector.Y);
        public void Offset(int X, int Y)
        {
            Left += X;
            Right += X;
            Top += Y;
            Bottom += Y;
        }

        public bool Contains(Int32Point Point)
            => Contains(Point.X, Point.Y);
        public bool Contains(int X, int Y)
        {
            if (IsEmpty)
                return false;

            return Left < X && X < Right &&
                   Top < Y && Y < Bottom;
        }

        public override string ToString()
            => $"{{ Left : {Left}, Top : {Top}, Right : {Right}, Bottom : {Bottom} }}";

    }
}
