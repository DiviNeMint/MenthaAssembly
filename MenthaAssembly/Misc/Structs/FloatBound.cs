﻿using System;

namespace MenthaAssembly
{
    public struct FloatBound
    {
        public static FloatBound Empty => new FloatBound();

        public float Left { set; get; }

        public float Top { set; get; }

        public float Right { set; get; }

        public float Bottom { set; get; }

        public float Width => Right - Left;

        public float Height => Bottom - Top;

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
        public bool IntersectsWith(FloatBound Bound)
        {
            if (IsEmpty || Bound.IsEmpty)
                return false;

            return Bound.Left <= Right &&
                   Bound.Right >= Left &&
                   Bound.Top <= Bottom &&
                   Bound.Bottom >= Top;
        }

        public void Union(FloatBound Bound)
        {
            if (IsEmpty)
            {
                this = Bound;
                return;
            }

            if (Bound.IsEmpty)
                return;

            Left = Math.Min(Left, Bound.Left);
            Right = Math.Max(Right, Bound.Right);
            Top = Math.Min(Top, Bound.Top);
            Bottom = Math.Max(Bottom, Bound.Bottom);
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

        public bool Contains(FloatPoint Point)
            => Contains(Point.X, Point.Y);
        public bool Contains(float X, float Y)
        {
            if (IsEmpty)
                return false;

            return Left < X && X < Right &&
                   Top < Y && Y < Bottom;
        }

        public override string ToString()
            => $"{{ Left : {Left}, Top : {Top}, Right : {Right}, Bottom : {Bottom} }}";

    }
}
