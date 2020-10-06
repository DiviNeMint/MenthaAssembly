namespace MenthaAssembly
{
    public struct FloatPoint
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

        public override string ToString()
            => $@"{{ X : {this.X}, Y : {this.Y} }}";
    }
}
