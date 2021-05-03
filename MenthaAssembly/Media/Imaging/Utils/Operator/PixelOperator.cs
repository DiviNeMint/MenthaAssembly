using System;
using System.Collections.Concurrent;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal unsafe class PixelOperator<Pixel> : IPixelOperator
        where Pixel : unmanaged, IPixel
    {
        internal unsafe delegate void PixelHandler(ref byte* pPixel, byte A, byte R, byte G, byte B);

        private PixelOperator()
        {
            Type TPixel = typeof(Pixel);
            int PixelSkipLength = sizeof(Pixel) - 1;

            if (TPixel == typeof(BGRA))
            {
                OverrideHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    *Pixel++ = B;
                    *Pixel++ = G;
                    *Pixel++ = R;
                    *Pixel = A;
                };
                OverlayHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    Pixel += PixelSkipLength;
                    
                    if (A == 0)
                        return;

                    int A1 = *Pixel,
                        rA = 255 - A,
                        Alpha = 65025 - rA * (255 - A1);

                    *Pixel-- = (byte)(Alpha / 255);
                    *Pixel-- = (byte)((R * A * 255 + *Pixel * A1 * rA) / Alpha);
                    *Pixel-- = (byte)((G * A * 255 + *Pixel * A1 * rA) / Alpha);
                    *Pixel = (byte)((B * A * 255 + *Pixel * A1 * rA) / Alpha);

                    Pixel += PixelSkipLength;
                };
            }
            else if (TPixel == typeof(RGBA))
            {
                OverrideHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    *Pixel++ = R;
                    *Pixel++ = G;
                    *Pixel++ = B;
                    *Pixel = A;
                };
                OverlayHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    Pixel += PixelSkipLength;

                    if (A == 0)
                        return;

                    int A1 = *Pixel,
                        rA = 255 - A,
                        Alpha = 65025 - rA * (255 - A1);

                    *Pixel-- = (byte)(Alpha / 255);
                    *Pixel-- = (byte)((B * A * 255 + *Pixel * A1 * rA) / Alpha);
                    *Pixel-- = (byte)((G * A * 255 + *Pixel * A1 * rA) / Alpha);
                    *Pixel = (byte)((R * A * 255 + *Pixel * A1 * rA) / Alpha);

                    Pixel += PixelSkipLength;
                };
            }
            else if (TPixel == typeof(ARGB))
            {
                OverrideHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    *Pixel++ = A;
                    *Pixel++ = R;
                    *Pixel++ = G;
                    *Pixel = B;
                };
                OverlayHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    if (A == 0)
                    {
                        Pixel += PixelSkipLength;
                        return;
                    }

                    int A1 = *Pixel,
                        rA = 255 - A,
                        Alpha = 65025 - rA * (255 - A1);

                    *Pixel++ = (byte)(Alpha / 255);
                    *Pixel++ = (byte)((R * A * 255 + *Pixel * A1 * rA) / Alpha);
                    *Pixel++ = (byte)((G * A * 255 + *Pixel * A1 * rA) / Alpha);
                    *Pixel = (byte)((B * A * 255 + *Pixel * A1 * rA) / Alpha);
                };
            }
            else if (TPixel == typeof(ABGR))
            {
                OverrideHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    *Pixel++ = A;
                    *Pixel++ = B;
                    *Pixel++ = G;
                    *Pixel = R;
                };
                OverlayHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    if (A == 0)
                    {
                        Pixel += PixelSkipLength;
                        return;
                    }

                    int A1 = *Pixel,
                        rA = 255 - A,
                        Alpha = 65025 - rA * (255 - A1);

                    *Pixel++ = (byte)(Alpha / 255);
                    *Pixel++ = (byte)((B * A * 255 + *Pixel * A1 * rA) / Alpha);
                    *Pixel++ = (byte)((G * A * 255 + *Pixel * A1 * rA) / Alpha);
                    *Pixel = (byte)((R * A * 255 + *Pixel * A1 * rA) / Alpha);
                };
            }
            else if (TPixel == typeof(BGR))
            {
                OverrideHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    *Pixel++ = B;
                    *Pixel++ = G;
                    *Pixel = R;
                };
                OverlayHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    if (A == 0)
                    {
                        Pixel += PixelSkipLength;
                        return;
                    }

                    int rA = 255 - A;

                    *Pixel++ = (byte)((B * A + *Pixel * rA) / 255);
                    *Pixel++ = (byte)((G * A + *Pixel * rA) / 255);
                    *Pixel = (byte)((R * A + *Pixel * rA) / 255);
                };
            }
            else if (TPixel == typeof(RGB))
            {
                OverrideHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    *Pixel++ = R;
                    *Pixel++ = G;
                    *Pixel = B;
                };
                OverlayHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    if (A == 0)
                    {
                        Pixel += PixelSkipLength;
                        return;
                    }

                    int rA = 255 - A;

                    *Pixel++ = (byte)((R * A + *Pixel * rA) / 255);
                    *Pixel++ = (byte)((G * A + *Pixel * rA) / 255);
                    *Pixel = (byte)((B * A + *Pixel * rA) / 255);
                };
            }
            else if (TPixel == typeof(Gray8))
            {
                OverrideHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                    *Pixel = (byte)((R * 30 +
                                     G * 59 +
                                     B * 11 + 50) / 100);

                OverlayHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    if (A == 0)
                    {
                        Pixel += PixelSkipLength;
                        return;
                    }

                    int rA = 255 - A,
                        Gray = *Pixel;

                    *Pixel = (byte)(((R * A + Gray * rA) / 255 * 30 +
                                     (G * A + Gray * rA) / 255 * 59 +
                                     (B * A + Gray * rA) / 255 * 11 + 50) / 100);
                };
            }
            else
            {
                OverrideHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    dynamic CopyHandler = new BGRA(B, G, R, A);
                    *(Pixel*)Pixel = (Pixel)CopyHandler;
                    Pixel += PixelSkipLength;
                };
                OverlayHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    if (A == 0)
                    {
                        Pixel += PixelSkipLength;
                        return;
                    }

                    Pixel Data = *(Pixel*)Pixel;
                    int A1 = Data.A,
                        rA = 255 - A,
                        Alpha = 65025 - rA * (255 - A1);

                    dynamic Result = new BGRA((byte)((B * A * 255 + Data.B * A1 * rA) / Alpha),
                                              (byte)((G * A * 255 + Data.G * A1 * rA) / Alpha), 
                                              (byte)((R * A * 255 + Data.R * A1 * rA) / Alpha), 
                                              (byte)(Alpha / 255));

                    *(Pixel*)Pixel = (Pixel)Result;
                    Pixel += PixelSkipLength;
                };
            }
        }

        public Pixel ToPixel(byte A, byte R, byte G, byte B)
        {
            Pixel Pixel = default;
            byte* pPixel = (byte*)&Pixel;
            this.OverrideHandler(ref pPixel, A, R, G, B);
            return Pixel;
        }

        private readonly PixelHandler OverrideHandler;
        public void Override(ref byte* pPixel, byte A, byte R, byte G, byte B)
            => this.OverrideHandler(ref pPixel, A, R, G, B);

        private readonly PixelHandler OverlayHandler;
        public void Overlay(ref byte* pPixel, byte A, byte R, byte G, byte B)
            => this.OverlayHandler(ref pPixel, A, R, G, B);
        public void Overlay(ref byte* pPixelA, ref byte* pPixelR, ref byte* pPixelG, ref byte* pPixelB, byte A, byte R, byte G, byte B)
        {
            int A1 = *pPixelA,
                rA = 255 - A,
                Alpha = 65025 - rA * (255 - A1);

            *pPixelA = (byte)(Alpha / 255);
            *pPixelR = (byte)((R * A * 255 + *pPixelR * A1 * rA) / Alpha);
            *pPixelG = (byte)((G * A * 255 + *pPixelG * A1 * rA) / Alpha);
            *pPixelB = (byte)((B * A * 255 + *pPixelB * A1 * rA) / Alpha);
        }
        public void Overlay(ref byte* pPixelR, ref byte* pPixelG, ref byte* pPixelB, byte A, byte R, byte G, byte B)
        {
            int rA = 255 - A;

            *pPixelR = (byte)((R * A + *pPixelR * rA) / 255);
            *pPixelG = (byte)((G * A + *pPixelG * rA) / 255);
            *pPixelB = (byte)((B * A + *pPixelB * rA) / 255);
        }
        private static readonly ConcurrentDictionary<Type, IPixelOperator> PixelOperators = new ConcurrentDictionary<Type, IPixelOperator>();
        public static PixelOperator<Pixel> GetOperator()
        {
            Type Key = typeof(Pixel);
            if (PixelOperators.TryGetValue(Key, out IPixelOperator IOperator))
                return (PixelOperator<Pixel>)IOperator;

            PixelOperator<Pixel> Operator = new PixelOperator<Pixel>();
            PixelOperators.AddOrUpdate(Key, Operator, (k, o) => Operator);

            return Operator;
        }

    }
}
