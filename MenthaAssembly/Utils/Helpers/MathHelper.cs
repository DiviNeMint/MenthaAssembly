﻿using System;
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
        /// π / 2
        /// </summary>
        public const double HalfPI = Math.PI / 2d;

        /// <summary>
        /// 2π
        /// </summary>
        public const double TwoPI = Math.PI * 2d;

        /// <summary>
        /// Returns the sine of the specified angle.
        /// </summary>
        /// <param name="Angle">Degree, usually denoted by °</param>
        public static double Sin(double Angle)
            => Math.Sin(Angle * UnitTheta);
        /// <summary>
        /// Returns the cosine of the specified angle.
        /// </summary>
        /// <param name="Angle">Degree, usually denoted by °</param>
        public static double Cos(double Angle)
            => Math.Cos(Angle * UnitTheta);
        /// <summary>
        /// Returns the tangent of the specified angle.
        /// </summary>
        /// <param name="Angle">Degree, usually denoted by °</param>
        public static double Tan(double Angle)
            => Math.Tan(Angle * UnitTheta);

        /// <summary>
        /// Returns the angle whose sine is the specified number.
        /// </summary>
        /// <param name="Value">A number representing a sine, where must be greater than or equal to -1, but less than or equal to 1.</param>
        /// <returns>-90° &lt;= Angle &lt;= 90° </returns>
        public static double Asin(double Value)
        {
            double Result = Math.Asin(Value);
            if (double.IsNaN(Result))
                throw new ArgumentException($"Value must be greater than or equal to -1, but less than or equal to 1.");

            return Result / UnitTheta;
        }
        /// <summary>
        /// Returns the angle whose cosine is the specified number.
        /// </summary>
        /// <param name="Value">A number representing a cosine, where must be greater than or equal to -1, but less than or equal to 1.</param>
        /// <returns>0° &lt;= Angle &lt; 180° </returns>
        public static double Acos(double Value)
        {
            double Result = Math.Acos(Value);
            if (double.IsNaN(Result))
                throw new ArgumentException($"Value must be greater than or equal to -1, but less than or equal to 1.");

            return Result / UnitTheta;
        }
        /// <summary>
        /// Returns the angle whose tangent is the specified number.
        /// </summary>
        /// <param name="Value">A number representing a tangent.</param>
        /// <returns>-90° &lt;= Angle &lt;= 90° </returns>
        public static double Atan(double Value)
        {
            double Result = Math.Atan(Value);
            if (double.IsNaN(Result))
                throw new ArgumentException($"Value is invalid.");

            return Result / UnitTheta;
        }
        /// <summary>
        /// Returns the angle whose tangent is the specified number.
        /// </summary>
        /// <param name="X">The x coordinate of a point.</param>
        /// <param name="Y">The y coordinate of a point.</param>
        /// <returns>-180° &lt;= Angle &lt;= 180° </returns>
        public static double Atan(double X, double Y)
        {
            double Result = Math.Atan2(Y, X);
            if (double.IsNaN(Result))
                throw new ArgumentException($"X or Y is invalid value.");

            return Result / UnitTheta;
        }

        /// <summary>
        /// Returns the smallest interval number that is greater than or equal to the specified interval number.
        /// </summary>
        /// <param name="This">A double-precision floating-point number.</param>
        /// <param name="Interval">The interval number.</param>
        public static double Ceiling(this double This, double Interval)
            => Math.Ceiling(This / Interval) * Interval;
        /// <summary>
        /// Returns the smallest interval number that is greater than or equal to the specified interval number.
        /// </summary>
        /// <param name="This">A decimal number.</param>
        /// <param name="Interval">The interval number.</param>
        public static decimal Ceiling(this decimal This, decimal Interval)
            => Math.Ceiling(This / Interval) * Interval;

        /// <summary>
        /// Returns the largest interval number less than or equal to the specified interval number.
        /// </summary>
        /// <param name="This">A double-precision floating-point number.</param>
        /// <param name="Interval">The interval number.</param>
        public static double Floor(this double This, double Interval)
            => Math.Floor(This / Interval) * Interval;
        /// <summary>
        /// Returns the largest interval number less than or equal to the specified interval number.
        /// </summary>
        /// <param name="This">A decimal number.</param>
        /// <param name="Interval">The interval number.</param>
        public static decimal Floor(this decimal This, decimal Interval)
            => Math.Floor(This / Interval) * Interval;

        /// <summary>
        /// Rounds a value to the nearest interval number.
        /// </summary>
        /// <param name="This">A double-precision floating-point number to be rounded.</param>
        /// <param name="Interval">The interval number.</param>
        public static double Round(this double This, double Interval)
            => Math.Round(This / Interval) * Interval;
        /// <summary>
        /// Rounds a value to the nearest interval number.
        /// </summary>
        /// <param name="This">A double-precision floating-point number to be rounded.</param>
        /// <param name="Interval">The interval number.</param>
        /// <param name="Mode">Specification for how to round d if it is midway between two other numbers.</param>
        public static double Round(this double This, double Interval, MidpointRounding Mode)
            => Math.Round(This / Interval, Mode) * Interval;
        /// <summary>
        /// Rounds a value to the nearest interval number.
        /// </summary>
        /// <param name="This">A decimal number to be rounded.</param>
        /// <param name="Interval">The interval number.</param>
        public static decimal Round(this decimal This, decimal Interval)
            => Math.Round(This / Interval) * Interval;
        /// <summary>
        /// Rounds a value to the nearest interval number.
        /// </summary>
        /// <param name="This">A decimal number to be rounded.</param>
        /// <param name="Interval">The interval number.</param>
        /// <param name="Mode">Specification for how to round d if it is midway between two other numbers.</param>
        public static decimal Round(this decimal This, decimal Interval, MidpointRounding Mode)
            => Math.Round(This / Interval, Mode) * Interval;

        /// <summary>
        /// Returns value clamped to the inclusive range of Min and Max.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="This">The value to be clamped.</param>
        /// <param name="Min">The lower bound of the result.</param>
        /// <param name="Max">The upper bound of the result.</param>
        /// <returns></returns>
        public static T Clamp<T>(this T This, T Min, T Max)
            where T : IComparable<T>
        {
            if (This.CompareTo(Min) < 0)
                return Min;

            if (This.CompareTo(Max) > 0)
                return Max;

            return This;
        }

        /// <summary>
        /// Returns the largest and the lowest number of specified numbers.
        /// </summary>
        /// <typeparam name="T">The type of number.</typeparam>
        /// <param name="Min">The lowest number.</param>
        /// <param name="Max">The largest number.</param>
        /// <param name="Source">The specified numbers.</param>
        public static void MinAndMax<T>(out T Min, out T Max, params T[] Source)
            where T : IComparable<T>
        {
            Source.MinAndMax(out Min, out Max);
        }
        /// <summary>
        /// Returns the largest and the lowest number of specified numbers.
        /// </summary>
        /// <typeparam name="T">The type of number.</typeparam>
        /// <param name="Source">The specified numbers.</param>
        /// <param name="Min">The lowest number.</param>
        /// <param name="Max">The largest number.</param>
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

        /// <summary>
        /// C(n,k)
        /// </summary>
        /// <returns>n! / k! * (n - k)!</returns>
        public static long Combination(int n, int k)
        {
            if (k > (n >> 1))
                k = n - k;

            if (k == 0)
                return 1;

            if (k == 1)
                return n;

            long t = 1;
            for (int i = 2; i <= k; i++)
                t *= i;

            long s = n--;
            for (int i = 1; i < k; i++, n--)
                s *= n;

            return s / t;
        }

        /// <summary>
        /// P(n,k)
        /// </summary>
        /// <returns>n! / (n - k)!</returns>
        public static int Permutation(int n, int k)
        {
            int s = n--;
            for (int i = 1; i < k; i++, n--)
                s *= n;

            return s;
        }

        /// <summary>
        /// Greatest Common Divisor
        /// </summary>
        public static int GCD(int a, int b)
        {
            int t;
            if (a < b)
            {
                t = b % a;
                return t > 0 ? GCD(a, t) : a;
            }

            t = a % b;
            return t > 0 ? GCD(b, t) : b;
        }

        /// <summary>
        /// Least Common Multiple
        /// </summary>
        public static int LCM(int a, int b)
            => a * b / GCD(a, b);

        /// <summary>
        /// Returns the absolute value of a specified number.
        /// </summary>
        /// <returns>| Value |</returns>
        public static int Abs(this int This)
        {
            if (This >= 0)
                return This;

            int Temp = This >> 31;
            return (This ^ Temp) - Temp;
        }

        /// <summary>
        /// Swap the numbers.
        /// </summary>
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
