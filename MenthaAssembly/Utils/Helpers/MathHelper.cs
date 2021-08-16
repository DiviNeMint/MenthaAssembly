using System;
using System.Collections.Generic;

namespace MenthaAssembly
{
    public static class MathHelper
    {
        /// <summary>
        /// π / 180
        /// </summary>
        public const double UnitTheta = Math.PI / 180d;

        /// <summary>
        /// 2π
        /// </summary>
        public const double TwoPI = Math.PI * 2d;

        public static double Sin(double Angle)
            => Math.Sin(Angle * UnitTheta);
        public static double Cos(double Angle)
            => Math.Cos(Angle * UnitTheta);
        public static double Tan(double Angle)
            => Math.Tan(Angle * UnitTheta);

        public static T Clamp<T>(this T This, T Min, T Max)
            where T : IComparable<T>
        {
            if (This.CompareTo(Min) < 0)
                return Min;

            if (This.CompareTo(Max) > 0)
                return Max;

            return This;
        }

        public static void MinAndMax<T>(out T Min, out T Max, params T[] Source)
            where T : IComparable<T>
        {
            Source.MinAndMax(out Min, out Max);
        }
        public static void MinAndMax<T>(this IEnumerable<T> Source, out T Min, out T Max)
            where T : IComparable<T>
        {
            IEnumerator<T> Enumerator = Source.GetEnumerator();

            if (!Enumerator.MoveNext())
            {
                Min = default;
                Max = default;
                return;
            }

            Min = Enumerator.Current;
            Max = Min;

            T Temp;
            while (Enumerator.MoveNext())
            {
                Temp = Enumerator.Current;

                if (Temp.CompareTo(Min) < 0)
                    Min = Temp;
                else if (Temp.CompareTo(Max) > 0)
                    Max = Temp;
            }
        }

        public static void Rotate(double Px, double Py, double Cx, double Cy, double Theta, out double Qx, out double Qy)
        {
            Rotate(Px - Cx, Py - Cy, Theta, out Qx, out Qy);
            Qx += Cx;
            Qy += Cy;
        }
        public static void Rotate(double Px, double Py, double Cx, double Cy, double Sin, double Cos, out double Qx, out double Qy)
        {
            Rotate(Px - Cx, Py - Cy, Sin, Cos, out Qx, out Qy);
            Qx += Cx;
            Qy += Cy;
        }
        public static void Rotate(double Px, double Py, double Theta, out double Qx, out double Qy)
        {
            double Sin = Math.Sin(Theta),
                   Cos = Math.Cos(Theta);

            Rotate(Px, Py, Sin, Cos, out Qx, out Qy);
        }
        public static void Rotate(double Px, double Py, double Sin, double Cos, out double Qx, out double Qy)
        {
            Qx = Px * Cos - Py * Sin;
            Qy = Px * Sin + Py * Cos;
        }

        /// <summary>
        /// Normalization Angle
        /// </summary>
        /// <returns>-180° &lt;= Angle &lt; 180° </returns>
        public static double NormalizationAngle(double Angle)
        {
            while (Angle > 180d)
                Angle -= 360d;

            while (Angle < -180d)
                Angle += 360d;

            return Angle;
        }
        /// <summary>
        /// Normalization Theta
        /// </summary>
        /// <returns>-π &lt;= θ &lt; π </returns>
        public static double NormalizationTheta(double Theta)
        {
            while (Theta > Math.PI)
                Theta -= TwoPI;

            while (Theta < -Math.PI)
                Theta += TwoPI;

            return Theta;
        }

        public static void CalculateCircle(double Px, double Py, double Qx, double Qy, double Rx, double Ry, out double Cx, out double Cy, out double Radius)
        {
            double Lp = Px * Px + Py * Py,
                   Lq = Qx * Qx + Qy * Qy,
                   Lr = Rx * Rx + Ry * Ry,
                   Xrq = Qx - Rx,
                   Xpr = Rx - Px,
                   Xqp = Px - Qx,
                   Yrq = Qy - Ry,
                   Ypr = Ry - Py,
                   Yqp = Py - Qy;

            Cx = (Lp * Yrq + Lq * Ypr + Lr * Yqp) / (2 * (Px * Yrq + Qx * Ypr + Rx * Yqp));
            Cy = (Lp * Xrq + Lq * Xpr + Lr * Xqp) / (2 * (Py * Xrq + Qy * Xpr + Ry * Xqp));

            Radius = Distance(Px, Py, Cx, Cy);
        }

        public static Bound<T> CalculateLineBound<T>(T X0, T Y0, T X1, T Y1)
            where T : struct
        {
            Func<T, T, bool> LessThan = Bound<T>.LessThan;
            return LessThan(X1, X0) ? (LessThan(Y1, Y0) ? new Bound<T>(X1, Y1, X0, Y0) :
                                                          new Bound<T>(X1, Y0, X0, Y1)) :
                                      (LessThan(Y1, Y0) ? new Bound<T>(X0, Y1, X1, Y0) :
                                                          new Bound<T>(X0, Y0, X1, Y1));
        }
        public static Bound<T> CalculateArcBound<T>(T Sx, T Sy, T Ex, T Ey, T Cx, T Cy, T Radius, bool Clockwise)
            where T : struct
        {
            Func<T, T, bool> LessThan = Bound<T>.LessThan;
            Func<T, T, T> Add = Bound<T>.Add,
                          Sub = Bound<T>.Sub;
            if (LessThan(Cx, Sx))
            {
                if (LessThan(Cy, Sy))
                {
                    if (LessThan(Cx, Ex))
                    {
                        if (LessThan(Cy, Ey))
                        {
                            bool Temp = LessThan(Sx, Ex);
                            return Clockwise == Temp ? new Bound<T>(Sub(Cx, Radius), Sub(Cy, Radius), Add(Cx, Radius), Add(Cy, Radius)) :
                                                       Temp ? new Bound<T>(Sx, Ey, Ex, Sy) :
                                                              new Bound<T>(Ex, Sy, Sx, Ey);
                        }
                        else
                        {
                            return Clockwise ? new Bound<T>(Sub(Cx, Radius), Sub(Cy, Radius), LessThan(Sx, Ex) ? Ex : Sx, Add(Cy, Radius)) :
                                               new Bound<T>(LessThan(Sx, Ex) ? Sx : Ex, Ey, Add(Cx, Radius), Sy);
                        }
                    }
                    else
                    {
                        return LessThan(Cy, Ey) ? (Clockwise ? new Bound<T>(Ex, LessThan(Sy, Ey) ? Sy : Ey, Sx, Add(Cy, Radius)) :
                                                      new Bound<T>(Sub(Cx, Radius), Sub(Cy, Radius), Add(Cx, Radius), LessThan(Sy, Ey) ? Ey : Sy)) :
                                         (Clockwise ? new Bound<T>(Sub(Cx, Radius), Ey, Sx, Add(Cy, Radius)) :
                                                      new Bound<T>(Ex, Sub(Cy, Radius), Add(Cx, Radius), Sy));
                    }
                }
                else
                {
                    if (LessThan(Cx, Ex))
                    {
                        if (LessThan(Cy, Ey))
                        {
                            return Clockwise ? new Bound<T>(LessThan(Sx, Ex) ? Sx : Ex, Sy, Add(Cx, Radius), Ey) :
                                               new Bound<T>(Sub(Cx, Radius), Sub(Cy, Radius), LessThan(Sx, Ex) ? Ex : Sx, Add(Cy, Radius));
                        }
                        else
                        {
                            bool Temp = LessThan(Sx, Ex);
                            return Clockwise == Temp ? (Temp ? new Bound<T>(Sx, Sy, Ex, Ey) :
                                                               new Bound<T>(Ex, Ey, Sx, Sy)) :
                                                       new Bound<T>(Sub(Cx, Radius), Sub(Cy, Radius), Add(Cx, Radius), Add(Cy, Radius));
                        }
                    }
                    else
                    {
                        return LessThan(Cy, Ey) ? (Clockwise ? new Bound<T>(Ex, Sy, Add(Cx, Radius), Add(Cy, Radius)) :
                                                      new Bound<T>(Sub(Cx, Radius), Sub(Cy, Radius), Sx, Ey)) :
                                         (Clockwise ? new Bound<T>(Sub(Cx, Radius), LessThan(Sy, Ey) ? Sy : Ey, Add(Cx, Radius), Add(Cy, Radius)) :
                                                      new Bound<T>(Ex, Sub(Cy, Radius), Sx, LessThan(Sy, Ey) ? Ey : Sy));
                    }
                }
            }
            else
            {
                if (LessThan(Cy, Sy))
                {
                    if (LessThan(Cx, Ex))
                    {
                        return LessThan(Cy, Ey) ? (Clockwise ? new Bound<T>(Sub(Cx, Radius), Sub(Cy, Radius), Add(Cx, Radius), LessThan(Sy, Ey) ? Ey : Sy) :
                                                      new Bound<T>(Sx, LessThan(Sy, Ey) ? Sy : Ey, Ex, Add(Cy, Radius))) :
                                         (Clockwise ? new Bound<T>(Sub(Cx, Radius), Sub(Cy, Radius), Ex, Sy) :
                                                      new Bound<T>(Sx, Ey, Add(Cx, Radius), Add(Cy, Radius)));
                    }
                    else
                    {
                        if (LessThan(Cy, Ey))
                        {
                            bool Temp = LessThan(Sx, Ex);
                            return Clockwise == Temp ? new Bound<T>(Sub(Cx, Radius), Sub(Cy, Radius), Add(Cx, Radius), Add(Cy, Radius)) :
                                                       Temp ? new Bound<T>(Sx, Sy, Ex, Ey) :
                                                              new Bound<T>(Ex, Ey, Sx, Sy);
                        }
                        else
                        {
                            return Clockwise ? new Bound<T>(Sub(Cx, Radius), Ey, LessThan(Sx, Ex) ? Ex : Sx, Sy) :
                                               new Bound<T>(LessThan(Sx, Ex) ? Sx : Ex, Sub(Cy, Radius), Add(Cx, Radius), Add(Cy, Radius));
                        }
                    }
                }
                else
                {
                    if (LessThan(Cx, Ex))
                    {
                        return LessThan(Cy, Ey) ? (Clockwise ? new Bound<T>(Sx, Sub(Cy, Radius), Add(Cx, Radius), Ey) :
                                                      new Bound<T>(Sub(Cx, Radius), Sy, Ex, Add(Cy, Radius))) :
                                         (Clockwise ? new Bound<T>(Sx, Sub(Cy, Radius), Ex, LessThan(Sy, Ey) ? Ey : Sy) :
                                                      new Bound<T>(Sub(Cx, Radius), LessThan(Sy, Ey) ? Sy : Ey, Add(Cx, Radius), Add(Cy, Radius)));
                    }
                    else
                    {
                        if (LessThan(Cy, Ey))
                        {
                            return Clockwise ? new Bound<T>(LessThan(Sx, Ex) ? Sx : Ex, Sub(Cy, Radius), Add(Cx, Radius), Add(Cy, Radius)) :
                                               new Bound<T>(Sub(Cx, Radius), Sy, LessThan(Sx, Ex) ? Ex : Sx, Ey);
                        }
                        else
                        {
                            bool Temp = LessThan(Sx, Ex);
                            return Clockwise == Temp ? (Temp ? new Bound<T>(Sx, Ey, Ex, Sy) :
                                                               new Bound<T>(Ex, Sy, Sx, Ey)) :
                                                       new Bound<T>(Sub(Cx, Radius), Sub(Cy, Radius), Add(Cx, Radius), Add(Cy, Radius));
                        }
                    }
                }
            }
        }
        public static Bound<T> CalculatePolygonBound<T>(IEnumerable<T> PointPairs)
            where T : struct
        {
            Func<T, T, bool> LessThan = Bound<T>.LessThan;

            IEnumerator<T> Enumerator = PointPairs.GetEnumerator();
            if (!Enumerator.MoveNext())
                return Bound<T>.Empty;

            T MinX = Enumerator.Current,
              MaxX = MinX;

            if (!Enumerator.MoveNext())
                return Bound<T>.Empty;

            T MinY = Enumerator.Current,
              MaxY = MinY,
              Temp;

            while (Enumerator.MoveNext())
            {
                Temp = Enumerator.Current;
                if (!Enumerator.MoveNext())
                    break;

                if (LessThan(Temp, MinX))
                    MinX = Temp;
                else if (LessThan(MaxX, Temp))
                    MaxX = Temp;

                Temp = Enumerator.Current;

                if (LessThan(Temp, MinY))
                    MinY = Temp;
                else if (LessThan(MaxY, Temp))
                    MaxY = Temp;
            }

            return new Bound<T>(MinX, MinY, MaxX, MaxY);
        }

        public static double Distance(double Px, double Py, double Qx, double Qy)
        {
            double Dx = Qx - Px,
                   Dy = Qy - Py;
            return Math.Sqrt(Dx * Dx + Dy * Dy);
        }

        public static int Abs(this int This)
        {
            if (This >= 0)
                return This;

            int Temp = This >> 31;
            return (This ^ Temp) - Temp;
        }

        public static void Swap(ref int X, ref int Y)
        {
            X ^= Y;
            Y ^= X;
            X ^= Y;
        }

        /// <summary>
        /// Calculate Reminder of <paramref name="Dividend"/> divided by 2 ^ <paramref name="N"/>
        /// </summary>
        public static int Reminder2(int Dividend, int N)
            => Dividend & ((1 << N) - 1);

        /// <summary>
        /// Calculate Reminder of <paramref name="Dividend"/> divided by 2 ^ <paramref name="N"/> - 1
        /// </summary>
        public static int Reminder3(int Dividend, int N)
        {
            int d = (1 << N) - 1,
                m;

            for (m = Dividend; Dividend > d; Dividend = m)
                for (m = 0; Dividend > 0; Dividend >>= N)
                    m += Dividend & d;

            // Now m is a value from 0 to d, but since with modulus division
            // we want m to be 0 when it is d.
            return m == d ? 0 : m;
        }

    }
}
