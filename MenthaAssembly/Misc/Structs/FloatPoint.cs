using System;

namespace MenthaAssembly
{
    public struct FloatPoint : ICloneable
    {
        public float X { set; get; }

        public float Y { set; get; }

        public bool IsOriginalPoint => X == 0f && Y == 0f;

        public FloatPoint(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }
        public FloatPoint(float X, float Y)
        {
            this.X = X;
            this.Y = Y;
        }
        public FloatPoint(double X, double Y) : this((float)X, (float)Y)
        {
        }

        public void Rotate(FloatPoint OriginalPoint, double Theta)
            => Rotate(OriginalPoint.X, OriginalPoint.Y, Theta);
        public void Rotate(float Ox, float Oy, double Theta)
        {
            MathHelper.Rotate(X, Y, Ox, Oy, Theta, out double Nx, out double Ny);
            this.X = (float)Nx;
            this.Y = (float)Ny;
        }
        public static FloatPoint Rotate(FloatPoint Point, FloatPoint OriginalPoint, double Theta)
            => Rotate(Point, OriginalPoint.X, OriginalPoint.Y, Theta);
        public static FloatPoint Rotate(FloatPoint Point, float Ox, float Oy, double Theta)
        {
            MathHelper.Rotate(Point.X, Point.Y, Ox, Oy, Theta, out double Nx, out double Ny);
            return new FloatPoint(Nx, Ny);
        }

        //public static FloatPoint operator +(FloatPoint This, FloatPoint Target)
        //    => new FloatPoint(This.X + Target.X, This.Y + Target.Y);

        //public static FloatPoint operator -(FloatPoint This, FloatPoint Target)
        //    => new FloatPoint(This.X - Target.X, This.Y - Target.Y);

        public static FloatPoint operator *(FloatPoint This, float Factor)
            => new FloatPoint(This.X * Factor, This.Y * Factor);
        public static FloatPoint operator *(FloatPoint This, double Factor)
            => new FloatPoint(This.X * Factor, This.Y * Factor);

        public static FloatPoint operator /(FloatPoint This, float Factor)
            => new FloatPoint(This.X / Factor, This.Y / Factor);
        public static FloatPoint operator /(FloatPoint This, double Factor)
            => new FloatPoint(This.X / Factor, This.Y / Factor);

        public static FloatPoint operator -(FloatPoint This)
            => new FloatPoint(-This.X, -This.Y);

        //public static FloatVector operator -(FloatPoint This, FloatPoint Target)
        //    => new FloatVector(This.X - Target.X, This.Y - Target.Y);

        public FloatPoint Clone()
            => new FloatPoint(this.X, this.Y);
        object ICloneable.Clone()
            => this.Clone();

        public override string ToString()
            => $@"{{ X : {this.X}, Y : {this.Y} }}";
    }
}
