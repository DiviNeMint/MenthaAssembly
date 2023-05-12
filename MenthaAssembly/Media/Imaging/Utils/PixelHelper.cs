using System;

namespace MenthaAssembly.Media.Imaging
{
    public static unsafe class PixelHelper
    {
        public static T ToPixel<T>(this IReadOnlyPixel Color) where T : unmanaged, IPixel
            => ToPixel<T>(Color.A, Color.R, Color.G, Color.B);
        public static T ToPixel<T>(byte A, byte R, byte G, byte B)
            where T : unmanaged, IPixel
        {
            T Pixel = default;
            Pixel.Override(A, R, G, B);
            return Pixel;
        }

        public static byte ToGray(this IReadOnlyPixel Pixel)
            => ToGray(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public static byte ToGray(byte A, byte R, byte G, byte B)
            => (byte)((R * 30 + G * 59 + B * 11 + 50) * A / 25500);

        public static void ToHSV(byte R, byte G, byte B, out double H, out double S, out double V)
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
            else if (Max == B)
                H = (R - G) * 60d / Delta + 240d;
            else
                H = 0d;
        }
        public static void ToRGB(double H, double S, double V, out byte R, out byte G, out byte B)
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

    }
}