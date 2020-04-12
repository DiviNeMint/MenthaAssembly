using System;
using System.Collections.Generic;
using System.Windows;

namespace MenthaAssembly
{
    public static class MathHelper
    {
        public const double UnitTheta = Math.PI / 180d;

        public static double Sin(double Angle)
            => Math.Sin(Angle * UnitTheta);
        public static double Cos(double Angle)
            => Math.Cos(Angle * UnitTheta);
        public static double Tan(double Angle)
            => Math.Tan(Angle * UnitTheta);

        public static double NormalizationAngle(double Angle)
        {
            while (Angle > 180d)
                Angle -= 360d;

            while (Angle < -180d)
                Angle += 360d;

            return Angle;
        }

        //public static void Circle(Point P, Point Q, Point R, out Point CenterPoint, out double Radius)
        //{
        //    double Lp = P.X * P.X + P.Y * P.Y,
        //           Lq = Q.X * Q.X + Q.Y * Q.Y,
        //           Lr = R.X * R.X + R.Y * R.Y,
        //           Xrq = Q.X - R.X,
        //           Xpr = R.X - P.X,
        //           Xqp = P.X - Q.X,
        //           Yrq = Q.Y - R.Y,
        //           Ypr = R.Y - P.Y,
        //           Yqp = P.Y - Q.Y;

        //    CenterPoint = new Point((Lp * Yrq + Lq * Ypr + Lr * Yqp) / (2 * (P.X * Yrq + Q.X * Ypr + R.X * Yqp)),
        //                            (Lp * Xrq + Lq * Xpr + Lr * Xqp) / (2 * (P.Y * Xrq + Q.Y * Xpr + R.Y * Xqp)));
        //    Radius = Math.Sqrt((P.X - CenterPoint.X) * (P.X - CenterPoint.X) + (P.Y - CenterPoint.Y) * (P.Y - CenterPoint.Y));
        //}

        //public static double Distance(Point P, Point Q)
        //{
        //    double DeltaX = Q.X - P.X,
        //           DeltaY = Q.Y - P.Y;
        //    return Math.Sqrt(DeltaX * DeltaX + DeltaY * DeltaY);
        //}

        public static IEnumerable<Int32Point> LinePoints(int X0, int Y0, int X1, int Y1, int Step)
        {
            if (Y0 > Y1)
            {
                Swap(ref X0, ref X1);
                Swap(ref Y0, ref Y1);
            }

            int DeltaX = X1 - X0;

            return LinePoints(X0, Y0, X1, Y1, Step, DeltaX, DeltaX.Abs(), Y1 - Y0);
        }
        internal static IEnumerable<Int32Point> LinePoints(int X0, int Y0, int X1, int Y1, int Step, int DeltaX, int AbsDeltaX, int AbsDeltaY)
        {
            int StepX = DeltaX > 0 ? Step : -Step,
                StepY = Step;

            if (AbsDeltaX < AbsDeltaY)
            {
                int Remainer = AbsDeltaY >> 1;
                do
                {
                    yield return new Int32Point(X0, Y0);
                    Remainer -= AbsDeltaX;
                    if (Remainer < 0)
                    {
                        Remainer += AbsDeltaY;
                        X0 += StepX;
                    }
                    Y0 += StepY;
                } while (Y0 <= Y1);
            }
            else
            {
                int Remainer = AbsDeltaX >> 1;

                if (X0 > X1)
                {
                    do
                    {
                        yield return new Int32Point(X0, Y0);
                        Remainer -= AbsDeltaY;
                        if (Remainer < 0)
                        {
                            Remainer += AbsDeltaX;
                            Y0 += StepY;
                        }
                        X0 += StepX;

                    } while (X0 >= X1);
                }
                else
                {
                    do
                    {
                        yield return new Int32Point(X0, Y0);
                        Remainer -= AbsDeltaY;
                        if (Remainer < 0)
                        {
                            Remainer += AbsDeltaX;
                            Y0 += StepY;
                        }
                        X0 += StepX;
                    } while (X0 <= X1);
                }
            }
        }


        //public static Point CrossPoint(Point LinePoint1, Point LinePoint2, Point OutsidePoint)
        //    => CrossPoint(LinePoint1,
        //                           (LinePoint2.Y - LinePoint1.Y) / (LinePoint2.X - LinePoint1.X),
        //                           OutsidePoint);
        //public static Point CrossPoint(Point LinePoint, double M, Point OutsidePoint)
        //{
        //    if (double.IsInfinity(M))
        //        return new Point(LinePoint.X, OutsidePoint.Y);

        //    double SquareM = M * M;
        //    return new Point((SquareM * LinePoint.X + M * (OutsidePoint.Y - LinePoint.Y) + OutsidePoint.X) / (SquareM + 1),
        //                     (SquareM * OutsidePoint.Y + M * (OutsidePoint.X - LinePoint.X) + LinePoint.Y) / (SquareM + 1));
        //}

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

        public static T MaxAndMin<T>(this T Value, T Max, T Min)
            where T : IComparable
        {
            int Result = Value.CompareTo(Min);
            return Result < 0 ? Min : (Result > 0 ? Max : Value);
        }

    }
}
