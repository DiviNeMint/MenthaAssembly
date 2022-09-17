using System;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public delegate bool ImagePredicate(int X, int Y, IReadOnlyPixel Pixel);
    public delegate void PixelAdapterAction<T>(PixelAdapter<T> Source) where T : unmanaged, IPixel;

    internal static unsafe class ImageOperatorEx
    {
        public static void ScanLine<T>(this IImageContext Context, int X, int Y, int Length, PixelAdapterAction<T> Handler)
            where T : unmanaged, IPixel
        {
            PixelAdapter<T> Adapter = Context.GetAdapter<T>(X, Y);
            for (int i = 0; i < Length; i++, Adapter.MoveNext())
                Handler(Adapter);
        }

        public static void Contour<T>(this IImageContext Context, IImageContour Contour, double OffsetX, double OffsetY, PixelAdapterAction<T> Handler)
            where T : unmanaged, IPixel
        {
            Contour.EnsureContents();
            int Dx = (int)Math.Round(Contour.OffsetX + OffsetX),
                Dy = (int)Math.Round(Contour.OffsetY + OffsetY);

            PixelAdapter<T> Adapter = Context.GetAdapter<T>(0, 0);
            int MaxX = Adapter.MaxX,
                MaxY = Adapter.MaxY,
                Ty;
            foreach (KeyValuePair<int, ImageContourScanLine> Content in Contour.Contents)
            {
                Ty = Content.Key + Dy;
                if (Ty < 0 || MaxY < Ty)
                    continue;

                Adapter.InternalMove(0, Ty);

                int CurrentX = 0;
                List<int> Data = Content.Value.Datas;
                for (int i = 0; i < Data.Count;)
                {
                    int Sx = Dx + Data[i++];
                    if (MaxX < Sx)
                        return;

                    int Ex = Dx + Data[i++];
                    if (Ex < 0)
                        continue;

                    Sx = Math.Max(Sx, 0);
                    Ex = Math.Min(Ex, MaxX);

                    Adapter.InternalMoveX(Sx - CurrentX);
                    for (int j = Sx; j <= Ex; j++, Adapter.InternalMoveNext())
                        Handler(Adapter);

                    CurrentX = Ex + 1;
                }
            }
        }

        public static ImageContour FindBound(this IReadOnlyImageContext Context, int SeedX, int SeedY, ImagePredicate Predicate)
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

            IReadOnlyPixelAdapter Seed, Pixel;
            while (StackX.Count > 0)
            {
                X = StackX.Pop();
                Y = StackY.Pop();
                SaveX = X;

                Seed = Context.GetAdapter(X, Y);
                Pixel = Seed.Clone();

                // Find Right Bound
                while (X < Width && !Predicate(X, Y, Pixel))
                {
                    X++;
                    Pixel.MoveNext();
                }

                // Find Left Bound
                Rx = X - 1;
                X = SaveX - 1;

                Pixel = Seed.Clone();
                Pixel.MovePrevious();
                while (-1 < X && !Predicate(X, Y, Pixel))
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

                Seed = Context.GetAdapter(X, Y);
                if (-1 < Y && Y < Height &&
                    !Contour.Contain(X, Y))
                    for (; X <= Rx; X++)
                    {
                        while (X <= Rx && !Predicate(X, Y, Seed))
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

                Seed = Context.GetAdapter(X, Y);
                if (0 <= Y && Y < Height &&
                    !Contour.Contain(X, Y))
                    for (; X <= Rx; X++)
                    {
                        while (X <= Rx && !Predicate(X, Y, Seed))
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