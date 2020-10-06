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

            #region OverrideHandler
            if (TPixel == typeof(BGRA))
            {
                OverrideHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    *Pixel++ = B;
                    *Pixel++ = G;
                    *Pixel++ = R;
                    *Pixel = A;
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
            }
            else if (TPixel == typeof(BGR))
            {
                OverrideHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    *Pixel++ = B;
                    *Pixel++ = G;
                    *Pixel = R;
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
            }
            else if (TPixel == typeof(Gray8))
            {
                OverrideHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                    *Pixel = (byte)((R * 30 +
                                     G * 59 +
                                     B * 11 + 50) / 100);
            }
            else
            {
                OverrideHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    dynamic CopyHandler = new BGRA(B, G, R, A);
                    *(Pixel*)Pixel = (Pixel)CopyHandler;
                    Pixel += PixelSkipLength;
                };
            }
            #endregion
            #region OverlayHandler
            if (TPixel == typeof(BGRA))
            {
                OverlayHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    if (A == 0)
                    {
                        Pixel += PixelSkipLength;
                        return;
                    }

                    //byte sb = *Pixel++,
                    //     sg = *Pixel++,
                    //     sr = *Pixel++,
                    //     sa = *Pixel;

                    //if (sa == byte.MaxValue)
                    //{
                    //    *Pixel-- = A;
                    //    *Pixel-- = R;
                    //    *Pixel-- = G;
                    //    *Pixel = B;
                    //}

                    *Pixel++ = B;
                    *Pixel++ = G;
                    *Pixel++ = R;
                    *Pixel = A;
                };
            }
            else if (TPixel == typeof(RGBA))
            {
                OverlayHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    if (A == 0)
                    {
                        Pixel += PixelSkipLength;
                        return;
                    }

                    *Pixel++ = R;
                    *Pixel++ = G;
                    *Pixel++ = B;
                    *Pixel = A;
                };
            }
            else if (TPixel == typeof(ARGB))
            {
                OverlayHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    if (A == 0)
                    {
                        Pixel += PixelSkipLength;
                        return;
                    }

                    *Pixel++ = A;
                    *Pixel++ = R;
                    *Pixel++ = G;
                    *Pixel = B;
                };
            }
            else if (TPixel == typeof(ABGR))
            {
                OverlayHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    if (A == 0)
                    {
                        Pixel += PixelSkipLength;
                        return;
                    }

                    *Pixel++ = A;
                    *Pixel++ = B;
                    *Pixel++ = G;
                    *Pixel = R;
                };
            }
            else if (TPixel == typeof(BGR))
            {
                OverlayHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    if (A == 0)
                    {
                        Pixel += PixelSkipLength;
                        return;
                    }

                    *Pixel++ = B;
                    *Pixel++ = G;
                    *Pixel = R;
                };
            }
            else if (TPixel == typeof(RGB))
            {
                OverlayHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    if (A == 0)
                    {
                        Pixel += PixelSkipLength;
                        return;
                    }

                    *Pixel++ = R;
                    *Pixel++ = G;
                    *Pixel = B;
                };
            }
            else if (TPixel == typeof(Gray8))
            {
                OverlayHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    if (A == 0)
                    {
                        Pixel += PixelSkipLength;
                        return;
                    }

                    *Pixel = (byte)((R * 30 +
                                     G * 59 +
                                     B * 11 + 50) / 100);
                };
            }
            else
            {
                OverlayHandler = (ref byte* Pixel, byte A, byte R, byte G, byte B) =>
                {
                    if (A == 0)
                    {
                        Pixel += PixelSkipLength;
                        return;
                    }

                    dynamic OverlayHandler = new BGRA(B, G, R, A);
                    *(Pixel*)Pixel = (Pixel)OverlayHandler;
                    Pixel += PixelSkipLength;
                };
            }
            #endregion
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
            *pPixelA = A;
            *pPixelR = R;
            *pPixelG = G;
            *pPixelB = B;
        }
        public void Overlay(ref byte* pPixelR, ref byte* pPixelG, ref byte* pPixelB, byte A, byte R, byte G, byte B)
        {
            *pPixelR = R;
            *pPixelG = G;
            *pPixelB = B;
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
