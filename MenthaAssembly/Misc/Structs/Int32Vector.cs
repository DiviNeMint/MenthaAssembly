namespace MenthaAssembly
{
    public struct Int32Vector
    {
        public int X { set; get; }

        public int Y { set; get; }

        public int LengthSquare => X * X + Y * Y;

        public bool IsZero => X is 0 && 
                              Y is 0;

        public Int32Vector(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public Int32Vector(double X, double Y) : this((int)X, (int)Y)
        {
        }

        public Int32Vector(Int32Point p1, Int32Point p2) : this(p2.X - p1.X, p2.Y - p2.Y)
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

        // Int32Point
        public static Int32Point operator +(Int32Point This, Int32Vector Vector) 
            => new Int32Point(This.X + Vector.X, This.Y + Vector.Y);

        public static Int32Point operator -(Int32Point This, Int32Vector Vector)
            => new Int32Point(This.X - Vector.X, This.Y - Vector.Y);

        public override string ToString()
            => $@"{{ X : {this.X}, Y : {this.Y} }}";
    }
}