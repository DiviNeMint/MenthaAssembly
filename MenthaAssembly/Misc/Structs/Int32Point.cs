namespace MenthaAssembly
{
    public struct Int32Point
    {
        public int X { get; }

        public int Y { get; }

        public bool IsOriginalPoint => X == 0 && Y == 0;

        public Int32Point(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public Int32Point(double X, double Y) : this((int)X, (int)Y)
        {
        }

        //public static Int32Point operator +(Int32Point This, Int32Point Target)
        //    => new Int32Point(This.X + Target.X, This.Y + Target.Y);

        //public static Int32Point operator -(Int32Point This, Int32Point Target)
        //    => new Int32Point(This.X - Target.X, This.Y - Target.Y);

        public static Int32Point operator *(Int32Point This, int Factor)
            => new Int32Point(This.X * Factor, This.Y * Factor);
        public static Int32Point operator *(Int32Point This, double Factor)
            => new Int32Point(This.X * Factor, This.Y * Factor);

        public static Int32Point operator /(Int32Point This, int Factor)
            => new Int32Point(This.X / Factor, This.Y / Factor);
        public static Int32Point operator /(Int32Point This, double Factor)
            => new Int32Point(This.X / Factor, This.Y / Factor);

        public static Int32Point operator -(Int32Point This)
            => new Int32Point(-This.X, -This.Y);

        public static Int32Vector operator -(Int32Point This, Int32Point Target)
            => new Int32Vector(This.X - Target.X, This.Y - Target.Y);

        public override string ToString()
            => $@"{{ X : {this.X}, Y : {this.Y} }}";
    }
}
