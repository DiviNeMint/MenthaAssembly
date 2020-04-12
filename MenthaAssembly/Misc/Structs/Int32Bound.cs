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

        public bool IsEmpty => Width.Equals(0) || Height.Equals(0);

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

    }
}
