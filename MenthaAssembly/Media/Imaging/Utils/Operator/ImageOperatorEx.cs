using System;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal static unsafe class ImageOperatorEx
    {
        public static T GetPixel<T>(this IImageContext Context, int X, int Y)
            where T : unmanaged, IPixel
        {
            T Pixel;
            Context.GetAdapter<T>(X, Y).OverrideTo(&Pixel);
            return Pixel;
        }

        public static void SetPixel<T>(this IImageContext Context, int X, int Y, T Pixel)
            where T : unmanaged, IPixel
            => Context.GetAdapter<T>(X, Y).Override(Pixel);

        public static void ScanLine<T>(this IImageContext Context, int X, int Y, int Length, PixelAdapterAction<T> Handler)
            where T : unmanaged, IPixel
        {
            PixelAdapter<T> Adapter = Context.GetAdapter<T>(X, Y);
            for (int i = 0; i < Length; i++, Adapter.MoveNext())
                Handler(Adapter);
        }
        public static void ScanLine<T>(this IImageContext Context, int X, int Y, int Length, PixelAdapterFunc<T, bool> Predicate)
            where T : unmanaged, IPixel
        {
            PixelAdapter<T> Adapter = Context.GetAdapter<T>(X, Y);
            for (int i = 0; i < Length; i++)
                if (Predicate(Adapter))
                    Adapter.MoveNext();
        }
        public static void ScanLine<T>(this IImageContext Context, int X, int Y, ImageContourScanLine Range, PixelAdapterAction<T> Handler)
            where T : unmanaged, IPixel
        {
            PixelAdapter<T> Adapter = Context.GetAdapter<T>(0, Y);
            List<int> Data = Range.Datas;

            int MaxX = Context.Width - 1;
            int CurrentX = 0;
            for (int i = 0; i < Data.Count;)
            {
                int Sx = X + Data[i++];
                if (MaxX < Sx)
                    return;

                int Ex = X + Data[i++];
                if (Ex < 0)
                    continue;

                Sx = Math.Max(Sx, 0);
                Ex = Math.Min(Ex, MaxX);

                Adapter.Move(Sx - CurrentX);
                for (int j = Sx; j <= Ex; j++, Adapter.MoveNext())
                    Handler(Adapter);

                CurrentX = Ex + 1;
            }
        }

        public static void ScanLineNearestResizeTo<T>(this IImageContext Context, int X, int Y, int Length, float FracX, float Step, PixelAdapter<T> Dest, PixelAdapterAction2<T> Handler)
            where T : unmanaged, IPixel
        {
            PixelAdapter<T> Sorc = Context.GetAdapter<T>(X, Y);
            for (int i = 0; i < Length; i++)
            {
                Handler(Sorc, Dest);
                Dest.MoveNext();

                FracX += Step;
                while (FracX >= 1f)
                {
                    FracX -= 1f;
                    Sorc.MoveNext();
                }
            }
        }

        public static void ScanLineBilinearResizeTo<T>(this IImageContext Context, int X, int Y, int Length, float FracX, float FracY, float Step, PixelAdapter<T> Dest, PixelAdapterAction3<T> Handler)
            where T : unmanaged, IPixel
        {
            PixelAdapter<T> p00 = Context.GetAdapter<T>(X, Y),
                             p10 = p00.Clone();

            if (Y + 1 < Context.Height)
                p10.MoveNextLine();

            float IFracY = 1f - FracY;
            int SourceW = Context.Width;
            for (int i = 0; i < Length; i++)
            {
                PixelAdapter<T> p01, p11;

                if (X < SourceW)
                {
                    p01 = p00.Clone();
                    p11 = p10.Clone();
                    p01.MoveNext();
                    p11.MoveNext();
                }
                else
                {
                    p01 = p00;
                    p11 = p10;
                }

                float IFracX = 1f - FracX,
                      IFxIFy = IFracX * IFracY,
                      IFxFy = IFracX * FracY,
                      FxIFy = FracX * IFracY,
                      FxFy = FracX * FracY;

                Handler(Dest,
                        (byte)(p00.A * IFxIFy + p01.A * FxIFy + p10.A * IFxFy + p11.A * FxFy),
                        (byte)(p00.R * IFxIFy + p01.R * FxIFy + p10.R * IFxFy + p11.R * FxFy),
                        (byte)(p00.G * IFxIFy + p01.G * FxIFy + p10.G * IFxFy + p11.G * FxFy),
                        (byte)(p00.B * IFxIFy + p01.B * FxIFy + p10.B * IFxFy + p11.B * FxFy));
                Dest.MoveNext();

                FracX += Step;
                while (FracX >= 1f)
                {
                    FracX -= 1f;

                    X++;
                    p00.MoveNext();
                    p10.MoveNext();
                }
            }
        }

        public static void Block<T>(this IImageContext<T> Context, int X, int Y, IImageContext Source, int SourceX, int SourceY, int Width, int Height, PixelAdapterAction2<T> Handler)
            where T : unmanaged, IPixel
        {
            PixelAdapter<T> Sorc = Source.GetAdapter<T>(SourceX, SourceY),
                             Dest = Context.GetAdapter<T>(X, Y);

            for (int j = 0; j < Height; j++, Sorc.MoveNextLine(), Dest.MoveNextLine())
            {
                for (int i = 0; i < Width; i++, Sorc.MoveNext(), Dest.MoveNext())
                    Handler(Sorc, Dest);

                Sorc.Move(-Width);
                Dest.Move(-Width);
            }
        }

        public static void Contour<T>(this IImageContext Context, IImageContour Contour, double OffsetX, double OffsetY, PixelAdapterAction<T> Handler)
            where T : unmanaged, IPixel
        {
            Contour.EnsureContents();
            int Dx = (int)Math.Round(Contour.OffsetX + OffsetX),
                Dy = (int)Math.Round(Contour.OffsetY + OffsetY);

            foreach (KeyValuePair<int, ImageContourScanLine> Content in Contour.Contents)
                ScanLine(Context, Dx, Content.Key + Dy, Content.Value, Handler);
        }

        public static ImageContour FindBound<T>(this IImageContext<T> Context, int SeedX, int SeedY, ImagePredicate Predicate)
            where T : unmanaged, IPixel
        {
            int Width = Context.Width,
                Height = Context.Height;

            if (SeedX < 0 || Width <= SeedX ||
                SeedY < 0 || Height <= SeedY)
                return null;

            ImageContour Contour = new ImageContour();
            Stack<int> StackX = new Stack<int>(),
                       StackY = new Stack<int>();
            StackX.Push(SeedX);
            StackY.Push(SeedY);

            int X, Y, SaveX, Rx, Lx;

            PixelAdapter<T> Seed, Pixel;
            while (StackX.Count > 0)
            {
                X = StackX.Pop();
                Y = StackY.Pop();
                SaveX = X;

                Seed = Context.GetAdapter<T>(X, Y);
                Pixel = Seed.Clone();

                // Find Right Bound
                while (X < Width && !Predicate(X, Y, Pixel.A, Pixel.R, Pixel.G, Pixel.B))
                {
                    X++;
                    Pixel.MoveNext();
                }

                // Find Left Bound
                Rx = X - 1;
                X = SaveX - 1;

                Pixel = Seed.Clone();
                Pixel.MovePrevious();
                while (-1 < X && !Predicate(X, Y, Pixel.A, Pixel.R, Pixel.G, Pixel.B))
                {
                    X--;
                    Pixel.MovePrevious();
                }

                Lx = X + 1;

                // Log Region
                Contour[Y].Union(Lx, Rx);

                // Lower ScanLine's Seed
                bool NeedFill = false;
                X = Lx;
                Y++;

                Seed = Context.GetAdapter<T>(X, Y);
                if (-1 < Y && Y < Height &&
                    !Contour.Contain(X, Y))
                    for (; X <= Rx; X++)
                    {
                        while (X <= Rx && !Predicate(X, Y, Seed.A, Seed.R, Seed.G, Seed.B))
                        {
                            NeedFill = true;
                            X++;
                            Seed.MoveNext();
                        }

                        if (NeedFill)
                        {
                            StackX.Push(X - 1);
                            StackY.Push(Y);
                            NeedFill = false;
                        }
                    }

                // Upper ScanLine's Seed
                NeedFill = false;
                X = Lx;
                Y -= 2;

                Seed = Context.GetAdapter<T>(X, Y);
                if (0 <= Y && Y < Height &&
                    !Contour.Contain(X, Y))
                    for (; X <= Rx; X++)
                    {
                        while (X <= Rx && !Predicate(X, Y, Seed.A, Seed.R, Seed.G, Seed.B))
                        {
                            NeedFill = true;
                            X++;
                            Seed.MoveNext();
                        }

                        if (NeedFill)
                        {
                            StackX.Push(X - 1);
                            StackY.Push(Y);
                            NeedFill = false;
                        }
                    }
            }

            return Contour;
        }

    }

}