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
