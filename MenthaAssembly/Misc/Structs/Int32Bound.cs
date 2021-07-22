//using System;

//namespace MenthaAssembly
//{
//    public struct Int32Bound : ICloneable
//    {
//        public static Int32Bound Empty => new Int32Bound();

//        public int Left { set; get; }

//        public int Top { set; get; }

//        public int Right { set; get; }

//        public int Bottom { set; get; }

//        public int Width => Right - Left;

//        public int Height => Bottom - Top;

//        public bool IsEmpty
//            => Width is 0 || Height is 0;

//        public Int32Bound(int Left, int Top, int Right, int Bottom)
//        {
//            this.Left = Left;
//            this.Top = Top;
//            this.Right = Right;
//            this.Bottom = Bottom;
//        }

//        public Int32Bound(double Left, double Top, double Right, double Bottom)
//        {
//            this.Left = (int)Left;
//            this.Top = (int)Top;
//            this.Right = (int)Right;
//            this.Bottom = (int)Bottom;
//        }

//        public Int32Bound(Point<int> Position, Size<int> Size)
//        {
//            this.Left = Position.X;
//            this.Top = Position.Y;
//            this.Right = Position.X + Size.Width;
//            this.Bottom = Position.Y + Size.Height;
//        }


//        public void Rotate(double Theta)
//            => Rotate((Left + Right) * 0.5d, (Top + Bottom) * 0.5d, Theta);
//        public void Rotate(double Ox, double Oy, double Theta)
//        {
//            MathHelper.Rotate(Left, Top, Theta, Ox, Oy, out double NewLeft, out double NewTop);
//            MathHelper.Rotate(Right, Bottom, Theta, Ox, Oy, out double NewRight, out double NewBottom);

//            MathHelper.MinAndMax(out int Min, out int Max, (int)Math.Ceiling(NewLeft), (int)Math.Ceiling(NewRight));
//            this.Left = Min;
//            this.Right = Max;

//            MathHelper.MinAndMax(out Min, out Max, (int)Math.Ceiling(NewTop), (int)Math.Ceiling(NewBottom));
//            this.Top = Min;
//            this.Bottom = Max;
//        }
//        public static Int32Bound Rotate(Int32Bound Bound, double Theta)
//            => Int32Bound.Rotate(Bound, (Bound.Left + Bound.Right) >> 1, (Bound.Top + Bound.Bottom) >> 1, Theta);
//        public static Int32Bound Rotate(Int32Bound Bound, double Ox, double Oy, double Theta)
//        {
//            Int32Bound R = new Int32Bound();

//            MathHelper.Rotate(Bound.Left, Bound.Top, Theta, Ox, Oy, out double NewLeft, out double NewTop);
//            MathHelper.Rotate(Bound.Right, Bound.Bottom, Theta, Ox, Oy, out double NewRight, out double NewBottom);

//            MathHelper.MinAndMax(out int Min, out int Max, (int)Math.Ceiling(NewLeft), (int)Math.Ceiling(NewRight));
//            R.Left = Min;
//            R.Right = Max;

//            MathHelper.MinAndMax(out Min, out Max, (int)Math.Ceiling(NewTop), (int)Math.Ceiling(NewBottom));
//            R.Top = Min;
//            R.Bottom = Max;

//            return R;
//        }

//        public void Intersect(Int32Bound Bound)
//        {
//            if (this.IntersectsWith(Bound))
//            {
//                Left = Math.Max(Left, Bound.Left);
//                Right = Math.Max(Math.Min(Right, Bound.Right), Left);
//                Top = Math.Max(Top, Bound.Top);
//                Bottom = Math.Max(Math.Min(Bottom, Bound.Bottom), Top);
//            }
//            else
//            {
//                this = Empty;
//            }
//        }
//        public static Int32Bound Intersect(Int32Bound Bound1, Int32Bound Bound2)
//        {
//            if (Bound1.IntersectsWith(Bound2))
//                return new Int32Bound(Math.Max(Bound1.Left, Bound2.Left),
//                                      Math.Max(Math.Min(Bound1.Right, Bound2.Right), Bound1.Left),
//                                      Math.Max(Bound1.Top, Bound2.Top),
//                                      Math.Max(Math.Min(Bound1.Bottom, Bound2.Bottom), Bound1.Top));

//            return Empty;
//        }

//        public bool IntersectsWith(Int32Bound Bound)
//        {
//            if (IsEmpty || Bound.IsEmpty)
//                return false;

//            return Bound.Left <= Right &&
//                   Bound.Right >= Left &&
//                   Bound.Top <= Bottom &&
//                   Bound.Bottom >= Top;
//        }

//        public void Union(Int32Bound Bound)
//        {
//            if (IsEmpty)
//            {
//                this = Bound;
//                return;
//            }

//            if (Bound.IsEmpty)
//                return;

//            Left = Math.Min(Left, Bound.Left);
//            Right = Math.Max(Right, Bound.Right);
//            Top = Math.Min(Top, Bound.Top);
//            Bottom = Math.Max(Bottom, Bound.Bottom);
//        }
//        public static Int32Bound Union(Int32Bound Bound1, Int32Bound Bound2)
//        {
//            if (Bound1.IsEmpty)
//                return new Int32Bound(Bound2.Left, Bound2.Top, Bound2.Right, Bound2.Bottom);

//            if (Bound2.IsEmpty)
//                return new Int32Bound(Bound1.Left, Bound1.Top, Bound1.Right, Bound1.Bottom);

//            return new Int32Bound(Math.Min(Bound1.Left, Bound2.Left),
//                                  Math.Min(Bound1.Top, Bound2.Top),
//                                  Math.Max(Bound1.Right, Bound2.Right),
//                                  Math.Max(Bound1.Bottom, Bound2.Bottom));
//        }

//        public void Offset(Vector<int> Vector)
//            => Offset(Vector.X, Vector.Y);
//        public void Offset(int X, int Y)
//        {
//            Left += X;
//            Right += X;
//            Top += Y;
//            Bottom += Y;
//        }
//        public static Int32Bound Offset(Int32Bound Bound, int X, int Y)
//            => new Int32Bound(Bound.Left + X, Bound.Top + Y, Bound.Right + X, Bound.Bottom + Y);

//        public void Scale(int Scale)
//            => this.Scale(Scale, Scale);
//        public void Scale(int XScale, int YScale)
//        {
//            Right = Left + Width * XScale;
//            Bottom = Top + Height * YScale;
//        }
//        public static Int32Bound Scale(Int32Bound Bound, int Scale)
//            => Int32Bound.Scale(Bound, Scale, Scale);
//        public static Int32Bound Scale(Int32Bound Bound, int XScale, int YScale)
//            => new Int32Bound(Bound.Left, Bound.Top, Bound.Left + Bound.Width * XScale, Bound.Top + Bound.Height * YScale);

//        public bool Contains(Point<int> Point)
//            => Contains(Point.X, Point.Y);
//        public bool Contains(int X, int Y)
//        {
//            if (IsEmpty)
//                return false;

//            return Left < X && X < Right &&
//                   Top < Y && Y < Bottom;
//        }

//        public Int32Bound Clone()
//            => new Int32Bound(this.Left, this.Top, this.Right, this.Bottom);
//        object ICloneable.Clone()
//            => this.Clone();

//        public override string ToString()
//            => $"{{ Left : {Left}, Top : {Top}, Right : {Right}, Bottom : {Bottom} }}";

//    }
//}
