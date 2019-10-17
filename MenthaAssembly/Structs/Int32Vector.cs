using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace MenthaAssembly
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


        public static Int32Vector operator +(Int32Vector This, Int32Vector Targe)
            => new Int32Vector(This.X + Targe.X, This.Y + Targe.Y);

        public static Int32Vector operator -(Int32Vector This)
            => new Int32Vector(-This.X, -This.Y);
        public static Int32Vector operator -(Int32Vector This, Int32Vector Targe)
            => new Int32Vector(This.X - Targe.X, This.Y - Targe.Y);


        public static Int32Vector operator *(Int32Vector This, int Factor)
            => new Int32Vector(This.X * Factor, This.Y * Factor);
        public static Int32Vector operator *(Int32Vector This, double Factor)
            => new Int32Vector(This.X * Factor, This.Y * Factor);

        public static Int32Vector operator /(Int32Vector This, int Factor)
            => new Int32Vector(This.X / Factor, This.Y / Factor);
        public static Int32Vector operator /(Int32Vector This, double Factor)
            => new Int32Vector(This.X / Factor, This.Y / Factor);

        // Rect
        public static Int32Rect operator +(Int32Rect This, Int32Vector Vector)
            => new Int32Rect(This.X + Vector.X, This.Y + Vector.Y, This.Width, This.Height);
        public static Int32Rect operator -(Int32Rect This, Int32Vector Vector)
            => new Int32Rect(This.X - Vector.X, This.Y - Vector.Y, This.Width, This.Height);

        // Point
        public static Int32Point operator +(Int32Point This, Int32Vector Vector)
            => new Int32Point(This.X + Vector.X, This.Y + Vector.Y);
        public static Int32Point operator -(Int32Point This, Int32Vector Vector)
            => new Int32Point(This.X - Vector.X, This.Y - Vector.Y);

        public override string ToString()
            => $@"{{ X : {this.X}, Y : {this.Y} }}";
    }
}
