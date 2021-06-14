using System;

namespace MenthaAssembly
{
    public struct FloatBound : ICloneable
    {
        public static FloatBound Empty => new FloatBound();

        public float Left { set; get; }

        public float Top { set; get; }

        public float Right { set; get; }

        public float Bottom { set; get; }

        public float Width => Right - Left;

        public float Height => Bottom - Top;

        public FloatPoint Center => new FloatPoint((Left + Right) * 0.5f, (Top + Bottom) * 0.5f);

        public bool IsEmpty
            => Width is 0 || Height is 0;

        public FloatBound(float Left, float Top, float Right, float Bottom)
        {
            this.Left = Left;
            this.Top = Top;
            this.Right = Right;
            this.Bottom = Bottom;
        }
        public FloatBound(double Left, double Top, double Right, double Bottom)
        {
            this.Left = (float)Left;
            this.Top = (float)Top;
            this.Right = (float)Right;
            this.Bottom = (float)Bottom;
        }
        //public FloatBound(FloatPoint Position, FloatSize Size)
        //{
        //    this.Left = Position.X;
        //    this.Top = Position.Y;
        //    this.Right = Position.X + Size.Width;
        //    this.Bottom = Position.Y + Size.Height;
        //}

        public void Rotate(double Theta)
            => Rotate((Left + Right) * 0.5d, (Top + Bottom) * 0.5d, Theta);
        public void Rotate(double Ox, double Oy, double Theta)
        {
            MathHelper.Rotate(Left, Top, Theta, Ox, Oy, out double X0, out double Y0);
            MathHelper.Rotate(Left, Bottom, Theta, Ox, Oy, out double X1, out double Y1);
            MathHelper.Rotate(Right, Top, Theta, Ox, Oy, out double X2, out double Y2);
            MathHelper.Rotate(Right, Bottom, Theta, Ox, Oy, out double X3, out double Y3);

            float Max, Min;
            MathHelper.MinAndMax(out Min, out Max, (float)X0, (float)X1, (float)X2, (float)X3);
            this.Left = Min;
            this.Right = Max;

            MathHelper.MinAndMax(out Min, out Max, (float)Y0, (float)Y1, (float)Y2, (float)Y3);
            this.Top = Min;
            this.Bottom = Max;
        }
        public static FloatBound Rotate(FloatBound Bound, double Theta)
            => FloatBound.Rotate(Bound, (Bound.Left + Bound.Right) * 0.5d, (Bound.Top + Bound.Bottom) * 0.5d, Theta);
        public static FloatBound Rotate(FloatBound Bound, double Ox, double Oy, double Theta)
        {
            FloatBound R = new FloatBound();

            MathHelper.Rotate(Bound.Left, Bound.Top, Theta, Ox, Oy, out double X0, out double Y0);
            MathHelper.Rotate(Bound.Left, Bound.Bottom, Theta, Ox, Oy, out double X1, out double Y1);
            MathHelper.Rotate(Bound.Right, Bound.Top, Theta, Ox, Oy, out double X2, out double Y2);
            MathHelper.Rotate(Bound.Right, Bound.Bottom, Theta, Ox, Oy, out double X3, out double Y3);

            float Max, Min;
            MathHelper.MinAndMax(out Min, out Max, (float)X0, (float)X1, (float)X2, (float)X3);
            R.Left = Min;
            R.Right = Max;

            MathHelper.MinAndMax(out Min, out Max, (float)Y0, (float)Y1, (float)Y2, (float)Y3);

            R.Top = Min;
            R.Bottom = Max;

            return R;
        }

        public void Intersect(FloatBound Bound)
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
        public static FloatBound Intersect(FloatBound Bound1, FloatBound Bound2)
        {
            if (Bound1.IntersectsWith(Bound2))
                return new FloatBound(Math.Max(Bound1.Left, Bound2.Left),
                                      Math.Max(Math.Min(Bound1.Right, Bound2.Right), Bound1.Left),
                                      Math.Max(Bound1.Top, Bound2.Top),
                                      Math.Max(Math.Min(Bound1.Bottom, Bound2.Bottom), Bound1.Top));

            return Empty;
        }

        public bool IntersectsWith(FloatBound Bound)
        {
            if (IsEmpty || Bound.IsEmpty)
                return false;

            return Bound.Left <= Right &&
                   Bound.Right >= Left &&
                   Bound.Top <= Bottom &&
                   Bound.Bottom >= Top;
        }
        public bool IntersectsWith(float Left, float Top, float Right, float Bottom)
        {
            if (IsEmpty || Right - Left is 0 || Bottom - Top is 0)
                return false;

            return Left <= this.Right &&
                   Right >= this.Left &&
                   Top <= this.Bottom &&
                   Bottom >= this.Top;
        }

        public void Union(FloatBound Bound)
        {
            //if (IsEmpty)
            //{
            //    this = Bound;
            //    return;
            //}

            //if (Bound.IsEmpty)
            //    return;

            Left = Math.Min(Left, Bound.Left);
            Right = Math.Max(Right, Bound.Right);
            Top = Math.Min(Top, Bound.Top);
            Bottom = Math.Max(Bottom, Bound.Bottom);
        }
        public void Union(float Left, float Top, float Right, float Bottom)
        {
            //if (IsEmpty)
            //{
            //    this = new FloatBound(Left, Top, Right, Bottom);
            //    return;
            //}

            //if (Right - Left is 0 || 
            //    Bottom - Top is 0)
            //    return;

            this.Left = Math.Min(this.Left, Left);
            this.Right = Math.Max(this.Right, Right);
            this.Top = Math.Min(this.Top, Top);
            this.Bottom = Math.Max(this.Bottom, Bottom);
        }
        public static FloatBound Union(FloatBound Bound1, FloatBound Bound2)
        {
            //if (Bound1.IsEmpty)
            //    return new FloatBound(Bound2.Left, Bound2.Top, Bound2.Right, Bound2.Bottom);

            //if (Bound2.IsEmpty)
            //    return new FloatBound(Bound1.Left, Bound1.Top, Bound1.Right, Bound1.Bottom);

            return new FloatBound(Math.Min(Bound1.Left, Bound2.Left),
                                  Math.Min(Bound1.Top, Bound2.Top),
                                  Math.Max(Bound1.Right, Bound2.Right),
                                  Math.Max(Bound1.Bottom, Bound2.Bottom));
        }

        //public void Offset(FloatVector Vector)
        //    => Offset(Vector.X, Vector.Y);
        public void Offset(float X, float Y)
        {
            Left += X;
            Right += X;
            Top += Y;
            Bottom += Y;
        }
        public static FloatBound Offset(FloatBound Bound, float X, float Y)
            => new FloatBound(Bound.Left + X, Bound.Top + Y, Bound.Right + X, Bound.Bottom + Y);

        public void Scale(float Scale)
            => this.Scale(Scale, Scale);
        public void Scale(float XScale, float YScale)
        {
            Right = Left + Width * XScale;
            Bottom = Top + Height * YScale;
        }
        public static FloatBound Scale(FloatBound Bound, float Scale)
            => FloatBound.Scale(Bound, Scale, Scale);
        public static FloatBound Scale(FloatBound Bound, float XScale, float YScale)
            => new FloatBound(Bound.Left, Bound.Top, Bound.Left + Bound.Width * XScale, Bound.Top + Bound.Height * YScale);

        public bool Contains(FloatPoint Point)
            => Contains(Point.X, Point.Y);
        public bool Contains(float X, float Y)
        {
            if (IsEmpty)
                return false;

            return Left < X && X < Right &&
                   Top < Y && Y < Bottom;
        }

        public FloatBound Clone()
            => new FloatBound(this.Left, this.Top, this.Right, this.Bottom);
        object ICloneable.Clone()
            => this.Clone();

        public override string ToString()
            => $"{{ Left : {Left}, Top : {Top}, Right : {Right}, Bottom : {Bottom} }}";

    }
}
