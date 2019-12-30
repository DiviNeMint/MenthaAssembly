namespace System.Windows
{
    public struct Int32Vector
    {
        public int X { get; }

        public int Y { get; }

        public Int32Vector(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public Int32Vector(double X, double Y) : this((int)X, (int)Y)
        {
        }

        public Int32Vector(Vector Vector) : this((int)Vector.X, (int)Vector.Y)
        {
        }

        public Int32Vector(Int32Point p1, Int32Point p2) : this(p2.X - p1.X, p2.Y - p2.Y)
        {
        }

        public Int32Vector(Point p1, Point p2) : this(p2.X - p1.X, p2.Y - p2.Y)
        {
        }

        // Int32Vector
        public static Int32Vector operator +(Int32Vector This, Int32Vector Target)
            => new Int32Vector(This.X + Target.X, This.Y + Target.Y);

        public static Int32Vector operator -(Int32Vector This)
            => new Int32Vector(-This.X, -This.Y);
        public static Int32Vector operator -(Int32Vector This, Int32Vector Target)
            => new Int32Vector(This.X - Target.X, This.Y - Target.Y);


        public static Int32Vector operator *(Int32Vector This, int Factor)
            => new Int32Vector(This.X * Factor, This.Y * Factor);
        public static Int32Vector operator *(Int32Vector This, double Factor)
            => new Int32Vector(This.X * Factor, This.Y * Factor);

        public static Int32Vector operator /(Int32Vector This, int Factor)
            => new Int32Vector(This.X / Factor, This.Y / Factor);
        public static Int32Vector operator /(Int32Vector This, double Factor)
            => new Int32Vector(This.X / Factor, This.Y / Factor);

        // Vector
        public static Vector operator +(Vector This, Int32Vector Target)
            => new Vector(This.X + Target.X, This.Y + Target.Y);
        public static Vector operator -(Vector This, Int32Vector Target)
            => new Vector(This.X - Target.X, This.Y - Target.Y);

        // Int32Rect
        public static Int32Rect operator +(Int32Rect This, Int32Vector Vector)
            => new Int32Rect(This.X + Vector.X, This.Y + Vector.Y, This.Width, This.Height);
        public static Int32Rect operator -(Int32Rect This, Int32Vector Vector)
            => new Int32Rect(This.X - Vector.X, This.Y - Vector.Y, This.Width, This.Height);

        // Rect
        public static Rect operator +(Rect This, Int32Vector Vector)
            => new Rect(This.X + Vector.X, This.Y + Vector.Y, This.Width, This.Height);
        public static Rect operator -(Rect This, Int32Vector Vector)
            => new Rect(This.X - Vector.X, This.Y - Vector.Y, This.Width, This.Height);

        // Int32Point
        public static Int32Point operator +(Int32Point This, Int32Vector Vector)
            => new Int32Point(This.X + Vector.X, This.Y + Vector.Y);
        public static Int32Point operator -(Int32Point This, Int32Vector Vector)
            => new Int32Point(This.X - Vector.X, This.Y - Vector.Y);

        // Point
        public static Point operator +(Point This, Int32Vector Vector)
            => new Point(This.X + Vector.X, This.Y + Vector.Y);
        public static Point operator -(Point This, Int32Vector Vector)
            => new Point(This.X - Vector.X, This.Y - Vector.Y);

        public override string ToString()
            => $@"{{ X : {this.X}, Y : {this.Y} }}";
    }
}