using System.Windows;

namespace MenthaAssembly
{
    public struct Int32Point
    {
        public int X { get; }

        public int Y { get; }

        public Int32Point(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public Int32Point(double X, double Y) : this((int)X, (int)Y)
        {
        }

        public Int32Point(Point Point) : this((int)Point.X, (int)Point.Y)
        {
        }

        public static Int32Point operator +(Int32Point This, Int32Point Targe)
            => new Int32Point(This.X + Targe.X, This.Y + Targe.Y);

        public static Int32Point operator -(Int32Point This, Int32Point Targe)
            => new Int32Point(This.X - Targe.X, This.Y - Targe.Y);

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
        public static Int32Point operator -(Int32Point This, Int32Vector Targe)
            => new Int32Point(This.X - Targe.X, This.Y - Targe.Y);




        public override string ToString()
            => $@"{{ X : {this.X}, Y : {this.Y} }}";
    }
}
