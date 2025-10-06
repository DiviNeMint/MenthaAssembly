using System;
using System.Collections.Generic;
using System.Text;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal static class ContourAlgorithms
    {
        public static void FillContour<T>(PixelAdapter<T> Context, IImageContour Contour, double OffsetX, double OffsetY, Action<PixelAdapter<T>> Handler)
            where T : unmanaged, IPixel
        {
            Contour.EnsureContents();
            int Dx = (int)Math.Round(Contour.OffsetX + OffsetX),
                Dy = (int)Math.Round(Contour.OffsetY + OffsetY);

            int MaxX = Context.XLength - 1,
                MaxY = Context.YLength - 1,
                Ty;
            foreach (KeyValuePair<int, ImageContourScanLine> Content in Contour.Contents)
            {
                Ty = Content.Key + Dy;
                if (Ty < 0 || MaxY < Ty)
                    continue;

                Context.DangerousMove(0, Ty);

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

                    Context.DangerousOffsetX(Sx - CurrentX);
                    for (int j = Sx; j <= Ex; j++, Context.DangerousMoveNextX())
                        Handler(Context);

                    CurrentX = Ex + 1;
                }
            }
        }

        public static IEnumerable<ImageContour> DetectContours(IImageContext Context, ImagePredicate SeedPredicate, ImagePredicate EdgePredicate)
        {
            List<ImageContour> Contours = [];
            bool ContourContain(int Sx, int Sy, out int Rx)
            {
                foreach (ImageContour Contour in Contours)
                {
                    if (!Contour.Contents.TryGetValue(Sy, out ImageContourScanLine ScanLine))
                        continue;

                    List<int> Datas = ScanLine.Datas;
                    for (int i = 1; i < ScanLine.Length; i += 2)
                    {
                        int TRx = Datas[i];
                        if (Datas[i - 1] <= Sx && Sx <= TRx)
                        {
                            Rx = TRx;
                            return true;
                        }
                    }
                }

                Rx = -1;
                return false;
            }

            int Iw = Context.Width,
                Ih = Context.Height;

            IPixelAdapter Adapter = Context.GetAdapter(0, 0);
            for (int y = 0; y < Ih; y++)
            {
                Adapter.DangerousMove(0, y);
                for (int x = 0; x < Iw; x++, Adapter.DangerousMoveNextX())
                {
                    IPixelAdapter Pixel = Adapter.Clone();
                    if (SeedPredicate(x, y, Pixel))
                    {
                        if (ContourContain(x, y, out int Rx))
                        {
                            Adapter.DangerousOffsetX(Rx - x);
                            x = Rx;
                            continue;
                        }

                        ImageContour Contour = DetectContour(Adapter, Iw, Ih, x, y, EdgePredicate);
                        Contours.Add(Contour);
                    }
                }
            }

            return Contours;
        }
        /// <summary>
        /// Uses a Flood Fill algorithm starting from the given seed coordinates (<paramref name="Sx"/>, <paramref name="Sy"/>)
        /// to detect an image contour within the specified <paramref name="Context"/>.
        /// </summary>
        /// <param name="Context">
        /// The pixel adapter providing access to the image data for reading and evaluation.
        /// </param>
        /// <param name="Iw">
        /// The image width (typically equals <paramref name="Context"/>.Width).
        /// </param>
        /// <param name="Ih">
        /// The image height (typically equals <paramref name="Context"/>.Height).
        /// </param>
        /// <param name="Sx">
        /// The seed X-coordinate (SeedX) used as the starting point of the Flood Fill algorithm.
        /// </param>
        /// <param name="Sy">
        /// The seed Y-coordinate (SeedY) used as the starting point of the Flood Fill algorithm.
        /// </param>
        /// <param name="EdgePredicate">
        /// A predicate function used to determine whether a pixel belongs to the contour boundary.
        /// </param>
        /// <returns>
        /// An <see cref="ImageContour"/> representing the detected boundary starting from the seed point.
        /// </returns>
        public static ImageContour DetectContour(IPixelAdapter Context, int Iw, int Ih, int Sx, int Sy, ImagePredicate EdgePredicate)
        {
            ImageContour Contour = new();
            Stack<int> StackX = new(),
                       StackY = new();
            StackX.Push(Sx);
            StackY.Push(Sy);

            int X, Y, SaveX, Rx, Lx;

            IPixelAdapter Seed, Pixel;
            while (StackX.Count > 0)
            {
                X = StackX.Pop();
                Y = StackY.Pop();
                SaveX = X;

                Seed = Context.Clone();
                Seed.Move(X, Y);
                Pixel = Seed.Clone();

                // Find Right Bound
                while (X < Iw && !EdgePredicate(X, Y, Pixel))
                {
                    X++;
                    Pixel.MoveNextX();
                }

                // Find Left Bound
                Rx = X - 1;
                X = SaveX - 1;

                Pixel = Seed.Clone();
                Pixel.MovePreviousX();
                while (-1 < X && !EdgePredicate(X, Y, Pixel))
                {
                    X--;
                    Pixel.MovePreviousX();
                }

                Lx = X + 1;

                // Log Region
                Contour[Y].Union(Lx, Rx);

                // Lower ScanLine's Seed
                bool NeedFill = false;
                X = Lx;
                Y++;

                Seed = Context.Clone();
                Seed.Move(X, Y);
                if (-1 < Y && Y < Ih && !Contour.Contain(X, Y))
                {
                    for (; X <= Rx; X++, Seed.MoveNextX())
                    {
                        while (X <= Rx && !EdgePredicate(X, Y, Seed))
                        {
                            NeedFill = true;
                            X++;
                            Seed.MoveNextX();
                        }

                        if (NeedFill)
                        {
                            StackX.Push(X - 1);
                            StackY.Push(Y);
                            NeedFill = false;
                        }
                    }
                }

                // Upper ScanLine's Seed
                NeedFill = false;
                X = Lx;
                Y -= 2;

                Seed = Context.Clone();
                Seed.Move(X, Y);
                if (0 <= Y && Y < Ih && !Contour.Contain(X, Y))
                {

                    for (; X <= Rx; X++, Seed.MoveNextX())
                    {
                        while (X <= Rx && !EdgePredicate(X, Y, Seed))
                        {
                            NeedFill = true;
                            X++;
                            Seed.MoveNextX();
                        }

                        if (NeedFill)
                        {
                            StackX.Push(X - 1);
                            StackY.Push(Y);
                            NeedFill = false;
                        }
                    }
                }
            }

            return Contour;
        }

    }
}