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

        public static void MinAndMax<T>(out T Min, out T Max, params T[] Source)
            where T : IComparable<T>
            => Source.MinAndMax(out Min, out Max);
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
        public static void Rotate(double Px, double Py, double Theta, out double Qx, out double Qy)
        {
            Qx = Px * Math.Cos(Theta) - Py * Math.Sin(Theta);
            Qy = Px * Math.Sin(Theta) + Py * Math.Cos(Theta);
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
                   Yqp = Py - Qy,
                   Dx, Dy;

            Cx = (Lp * Yrq + Lq * Ypr + Lr * Yqp) / (2 * (Px * Yrq + Qx * Ypr + Rx * Yqp));
            Cy = (Lp * Xrq + Lq * Xpr + Lr * Xqp) / (2 * (Py * Xrq + Qy * Xpr + Ry * Xqp));

            Dx = Px - Cx;
            Dy = Py - Cy;
            Radius = Math.Sqrt(Dx * Dx + Dy * Dy);
        }

        public static FloatBound CalculateLineBound(float X0, float Y0, float X1, float Y1)
            => X1 < X0 ? (Y1 < Y0 ? new FloatBound(X1, Y1, X0, Y0) :
                                    new FloatBound(X1, Y0, X0, Y1)) :
                         (Y1 < Y0 ? new FloatBound(X0, Y1, X1, Y0) :
                                    new FloatBound(X0, Y0, X1, Y1));
        public static FloatBound CalculateArcBound(float Sx, float Sy, float Ex, float Ey, float Cx, float Cy, float Radius, bool Clockwise)
        {
            if (Cx < Sx)
            {
                if (Cy < Sy)
                {
                    if (Cx < Ex)
                    {
                        if (Cy < Ey)
                        {
                            bool Temp = Sx < Ex;
                            return Clockwise == Temp ? new FloatBound(Cx - Radius, Cy - Radius, Cx + Radius, Cy + Radius) :
                                                       Temp ? new FloatBound(Sx, Ey, Ex, Sy) :
                                                              new FloatBound(Ex, Sy, Sx, Ey);
                        }
                        else
                        {
                            return Clockwise ? new FloatBound(Cx - Radius, Cy - Radius, Sx < Ex ? Ex : Sx, Cy + Radius) :
                                               new FloatBound(Sx < Ex ? Sx : Ex, Ey, Cx + Radius, Sy);
                        }
                    }
                    else
                    {
                        return Cy < Ey ? (Clockwise ? new FloatBound(Ex, Sy < Ey ? Sy : Ey, Sx, Cy + Radius) :
                                                      new FloatBound(Cx - Radius, Cy - Radius, Cx + Radius, Sy < Ey ? Ey : Sy)) :
                                         (Clockwise ? new FloatBound(Cx - Radius, Ey, Sx, Cy + Radius) :
                                                      new FloatBound(Ex, Cy - Radius, Cx + Radius, Sy));
                    }
                }
                else
                {
                    if (Cx < Ex)
                    {
                        if (Cy < Ey)
                        {
                            return Clockwise ? new FloatBound(Sx < Ex ? Sx : Ex, Sy, Cx + Radius, Ey) :
                                               new FloatBound(Cx - Radius, Cy - Radius, Sx < Ex ? Ex : Sx, Cy + Radius);
                        }
                        else
                        {
                            bool Temp = Sx < Ex;
                            return Clockwise == Temp ? (Temp ? new FloatBound(Sx, Sy, Ex, Ey) :
                                                               new FloatBound(Ex, Ey, Sx, Sy)) :
                                                       new FloatBound(Cx - Radius, Cy - Radius, Cx + Radius, Cy + Radius);
                        }
                    }
                    else
                    {
                        return Cy < Ey ? (Clockwise ? new FloatBound(Ex, Sy, Cx + Radius, Cy + Radius) :
                                                      new FloatBound(Cx - Radius, Cy - Radius, Sx, Ey)) :
                                         (Clockwise ? new FloatBound(Cx - Radius, Sy < Ey ? Sy : Ey, Cx + Radius, Cy + Radius) :
                                                      new FloatBound(Ex, Cy - Radius, Sx, Sy < Ey ? Ey : Sy));
                    }
                }
            }
            else
            {
                if (Cy < Sy)
                {
                    if (Cx < Ex)
                    {
                        return Cy < Ey ? (Clockwise ? new FloatBound(Cx - Radius, Cy - Radius, Cx + Radius, Sy < Ey ? Ey : Sy) :
                                                      new FloatBound(Sx, Sy < Ey ? Sy : Ey, Ex, Cy + Radius)) :
                                         (Clockwise ? new FloatBound(Cx - Radius, Cy - Radius, Ex, Sy) :
                                                      new FloatBound(Sx, Ey, Cx + Radius, Cy + Radius));
                    }
                    else
                    {
                        if (Cy < Ey)
                        {
                            bool Temp = Sx < Ex;
                            return Clockwise == Temp ? new FloatBound(Cx - Radius, Cy - Radius, Cx + Radius, Cy + Radius) :
                                                       Temp ? new FloatBound(Sx, Sy, Ex, Ey) :
                                                              new FloatBound(Ex, Ey, Sx, Sy);
                        }
                        else
                        {
                            return Clockwise ? new FloatBound(Cx - Radius, Ey, Sx < Ex ? Ex : Sx, Sy) :
                                               new FloatBound(Sx < Ex ? Sx : Ex, Cy - Radius, Cx + Radius, Cy + Radius);
                        }
                    }
                }
                else
                {
                    if (Cx < Ex)
                    {
                        return Cy < Ey ? (Clockwise ? new FloatBound(Sx, Cy - Radius, Cx + Radius, Ey) :
                                                      new FloatBound(Cx - Radius, Sy, Ex, Cy + Radius)) :
                                         (Clockwise ? new FloatBound(Sx, Cy - Radius, Ex, Sy < Ey ? Ey : Sy) :
                                                      new FloatBound(Cx - Radius, Sy < Ey ? Sy : Ey, Cx + Radius, Cy + Radius));
                    }
                    else
                    {
                        if (Cy < Ey)
                        {
                            return Clockwise ? new FloatBound(Sx < Ex ? Sx : Ex, Cy - Radius, Cx + Radius, Cy + Radius) :
                                               new FloatBound(Cx - Radius, Sy, Sx < Ex ? Ex : Sx, Ey);
                        }
                        else
                        {
                            bool Temp = Sx < Ex;
                            return Clockwise == Temp ? (Temp ? new FloatBound(Sx, Ey, Ex, Sy) :
                                                               new FloatBound(Ex, Sy, Sx, Ey)) :
                                                       new FloatBound(Cx - Radius, Cy - Radius, Cx + Radius, Cy + Radius);
                        }
                    }
                }
            }
        }
        public static FloatBound CalculatePolgonBound(IEnumerable<float> PointPairs)
        {
            IEnumerator<float> Enumerator = PointPairs.GetEnumerator();
            if (!Enumerator.MoveNext())
                return FloatBound.Empty;

            float MinX = Enumerator.Current,
                  MaxX = MinX;

            if (!Enumerator.MoveNext())
                return FloatBound.Empty;

            float MinY = Enumerator.Current,
                  MaxY = MinY,
                  Temp;

            while (Enumerator.MoveNext())
            {
                Temp = Enumerator.Current;
                if (!Enumerator.MoveNext())
                    break;

                if (Temp < MinX)
                    MinX = Temp;
                else if (MaxX < Temp)
                    MaxX = Temp;

                Temp = Enumerator.Current;

                if (Temp < MinY)
                    MinY = Temp;
                else if (MaxY < Temp)
                    MaxY = Temp;
            }

            return new FloatBound(MinX, MinY, MaxX, MaxY);
        }

        public static double Distance(double Px, double Py, double Qx, double Qy)
        {
            double DeltaX = Qx - Px,
                   DeltaY = Qy - Py;
            return Math.Sqrt(DeltaX * DeltaX + DeltaY * DeltaY);
        }

        public static void CrossPoint(double LinePx1, double LinePy1, double LinePx2, double LinePy2, double OutsidePx, double OutsidePy, out double CrossPx, out double CrossPy)
            => CrossPoint(LinePx1, LinePy1,
                          (LinePy2 - LinePy1) / (LinePx2 - LinePx1),
                          OutsidePx, OutsidePy,
                          out CrossPx, out CrossPy);
        public static void CrossPoint(double LinePx, double LinePy, double M, double OutsidePx, double OutsidePy, out double CrossPx, out double CrossPy)
        {
            if (double.IsInfinity(M))
            {
                CrossPx = LinePx;
                CrossPy = OutsidePy;
                return;
            }

            double SquareM = M * M;
            CrossPx = (SquareM * LinePx + M * (OutsidePy - LinePy) + OutsidePx) / (SquareM + 1);
            CrossPy = (SquareM * OutsidePy + M * (OutsidePx - LinePx) + LinePy) / (SquareM + 1);
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
