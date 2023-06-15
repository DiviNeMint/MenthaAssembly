using System;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging
{
    public static unsafe class PixelHelper
    {
        /// <summary>
        /// Creates a new pixel of the specified type.
        /// </summary>
        /// <typeparam name="T">The specified type.</typeparam>
        /// <param name="Color">The initial value.</param>
        public static T ToPixel<T>(this IReadOnlyPixel Color) where T : unmanaged, IPixel
            => ToPixel<T>(Color.A, Color.R, Color.G, Color.B);
        /// <summary>
        /// Creates a new pixel of the specified type.
        /// </summary>
        /// <typeparam name="T">The specified type.</typeparam>
        /// <param name="A">The initial alpha component value.</param>
        /// <param name="R">The initial red component value.</param>
        /// <param name="G">The initial green component value.</param>
        /// <param name="B">The initial blue component value.</param>
        public static T ToPixel<T>(byte A, byte R, byte G, byte B)
            where T : unmanaged, IPixel
        {
            T Pixel = default;
            Pixel.Override(A, R, G, B);
            return Pixel;
        }

        /// <summary>
        /// Calculates the grayscale of the specified pixel.
        /// </summary>
        /// <param name="Pixel">The specified pixel.</param>
        public static byte ToGray(this IReadOnlyPixel Pixel)
            => ToGray65536(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        /// <summary>
        /// Calculates the grayscale of the specified color components.
        /// </summary>
        /// <param name="A">The specified alpha component.</param>
        /// <param name="R">The specified red component.</param>
        /// <param name="G">The specified green component.</param>
        /// <param name="B">The specified blue component.</param>
        public static byte ToGray(byte A, byte R, byte G, byte B)
            => ToGray65536(A, R, G, B);

        /// <summary>
        /// (( R + G * 2 + B ) >> 2 ) * (A / 255)
        /// </summary>
        public static byte ToGray4(byte A, byte R, byte G, byte B)
            => A == byte.MaxValue ? R == G && G == B ? R : (byte)((R + (G << 1) + B) >> 2) :
                                    (byte)(((R + (G << 1) + B) >> 2) * A / 255);
        /// <summary>
        /// (( R * 2 + G * 5 + B ) >> 3 ) * (A / 255)
        /// </summary>
        public static byte ToGray8(byte A, byte R, byte G, byte B)
            => A == byte.MaxValue ? R == G && G == B ? R : (byte)(((R << 1) + G * 5 + B) >> 3) :
                                    (byte)((((R << 1) + G * 5 + B) >> 3) * A / 255);
        /// <summary>
        /// (( R * 38 + G * 75 + B * 15) >> 7 ) * (A / 255)
        /// </summary>
        public static byte ToGray128(byte A, byte R, byte G, byte B)
            => A == byte.MaxValue ? R == G && G == B ? R : (byte)((R * 38 + G * 75 + B * 15) >> 7) :
                                    (byte)(((R * 38 + G * 75 + B * 15) >> 7) * A / 255);
        /// <summary>
        /// (R * 30 + G * 59 + B * 11 + 50) * A / 25500
        /// </summary>
        public static byte ToGray100(byte A, byte R, byte G, byte B)
            => (byte)((R * 30 + G * 59 + B * 11 + 50) * A / 25500);
        /// <summary>
        /// (( R * 306 + G * 601 + B * 117) >> 10 ) * (A / 255)
        /// </summary>
        public static byte ToGray1024(byte A, byte R, byte G, byte B)
            => A == byte.MaxValue ? R == G && G == B ? R : (byte)((R * 306 + G * 601 + B * 117) >> 10) :
                                    (byte)(((R * 306 + G * 601 + B * 117) >> 10) * A / 255);
        /// <summary>
        /// (( R * 19595 + G * 38469 + B * 7472) >> 16 ) * (A / 255)
        /// </summary>
        public static byte ToGray65536(byte A, byte R, byte G, byte B)
            => A == byte.MaxValue ? R == G && G == B ? R : (byte)((R * 19595 + G * 38469 + B * 7472) >> 16) :
                                    (byte)(((R * 19595 + G * 38469 + B * 7472) >> 16) * A / 255);

        public static void GetHSV(byte R, byte G, byte B, out double H, out double S, out double V)
        {
            MathHelper.MinAndMax(out byte Max, out byte Min, R, G, B);

            double Delta = Max - Min;
            V = Max / 255d;
            S = Max == byte.MinValue ? 0d : Delta / Max;

            if (Delta == 0d)
                H = 0d;
            else if (Max == R && G >= B)
                H = (G - B) * 60d / Delta;
            else if (Max == R && G < B)
                H = (G - B) * 60d / Delta + 360d;
            else if (Max == G)
                H = (B - R) * 60d / Delta + 120d;
            else H = Max == B ? (R - G) * 60d / Delta + 240d : 0d;
        }
        public static void GetRGB(double H, double S, double V, out byte R, out byte G, out byte B)
        {
            V *= 255d;
            if (S <= 0d)
            {
                R = G = B = (byte)Math.Round(V);
                return;
            }

            double hh = H / 60d;
            switch (Math.Floor(hh))
            {
                case 1:
                    {
                        R = (byte)Math.Round(V * (1d - S * (hh - 1d)));
                        G = (byte)Math.Round(V);
                        B = (byte)Math.Round(V * (1d - S));
                        break;
                    }
                case 2:
                    {
                        R = (byte)Math.Round(V * (1d - S));
                        G = (byte)Math.Round(V);
                        B = (byte)Math.Round(V * (1d - S * (3d - hh)));
                        break;
                    }
                case 3:
                    {
                        R = (byte)Math.Round(V * (1d - S));
                        G = (byte)Math.Round(V * (1d - S * (hh - 3d)));
                        B = (byte)Math.Round(V);
                        break;
                    }
                case 4:
                    {
                        R = (byte)Math.Round(V * (1d - S * (5d - hh)));
                        G = (byte)Math.Round(V * (1d - S));
                        B = (byte)Math.Round(V);
                        break;
                    }
                case 5:
                    {
                        R = (byte)Math.Round(V);
                        G = (byte)Math.Round(V * (1d - S));
                        B = (byte)Math.Round(V * (1d - S * (hh - 5d)));
                        break;
                    }
                case 0:
                default:
                    {
                        R = (byte)Math.Round(V);
                        G = (byte)Math.Round(V * (1d - S * (1d - hh)));
                        B = (byte)Math.Round(V * (1d - S));
                    }
                    break;
            }
        }

        public static void Overlay(ref byte* pDestR, ref byte* pDestG, ref byte* pDestB, byte A, byte R, byte G, byte B)
        {
            int rA = 255 - A;
            *pDestR = (byte)((R * A + *pDestR * rA) / 255);
            *pDestG = (byte)((G * A + *pDestG * rA) / 255);
            *pDestB = (byte)((B * A + *pDestB * rA) / 255);
        }
        public static void Overlay(ref byte* pDestA, ref byte* pDestR, ref byte* pDestG, ref byte* pDestB, byte A, byte R, byte G, byte B)
        {
            int A1 = *pDestA,
                rA = 255 - A,
                Alpha = 65025 - rA * (255 - A1);

            *pDestA = (byte)(Alpha / 255);
            *pDestR = (byte)((R * A * 255 + *pDestR * A1 * rA) / Alpha);
            *pDestG = (byte)((G * A * 255 + *pDestG * A1 * rA) / Alpha);
            *pDestB = (byte)((B * A * 255 + *pDestB * A1 * rA) / Alpha);
        }

        private static readonly ConcurrentCollection<Type> NonAlphaPixelTypes = new() { typeof(Gray8), typeof(RGB), typeof(BGR), typeof(HSB), };
        /// <summary>
        /// Determines whether the specified pixel type is applied <see cref="NonAlphaAttribute"/>.
        /// </summary>
        public static bool IsNonAlphaPixel(Type PixelType)
        {
            if (NonAlphaPixelTypes.Contains(PixelType))
                return true;

            if (PixelType.GetCustomAttributes(typeof(NonAlphaAttribute), true).Length > 0)
            {
                NonAlphaPixelTypes.Add(PixelType);
                return true;
            }

            return false;
        }

        private static readonly ConcurrentCollection<Type> CalculatedPixelTypes = new() { typeof(Gray8) };
        /// <summary>
        /// Determines whether the specified pixel type is applied <see cref="CalculatedAttribute"/>.
        /// </summary>
        public static bool IsCalculatedPixel(Type PixelType)
        {
            if (CalculatedPixelTypes.Contains(PixelType))
                return true;

            if (PixelType.GetCustomAttributes(typeof(CalculatedAttribute), true).Length > 0)
            {
                CalculatedPixelTypes.Add(PixelType);
                return true;
            }

            return false;
        }

    }
}