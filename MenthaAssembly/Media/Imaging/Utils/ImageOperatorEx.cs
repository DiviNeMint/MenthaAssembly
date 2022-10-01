using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public delegate bool ImagePredicate(int X, int Y, IReadOnlyPixel Pixel);
    internal delegate void RefAction<T>(ref T Parameter);

    internal static unsafe class ImageOperatorEx
    {
        public static void ScanLine<T>(this IImageContext Context, int X, int Y, int Length, Action<PixelAdapter<T>> Handler)
            where T : unmanaged, IPixel
        {
            PixelAdapter<T> Adapter = Context.GetAdapter<T>(X, Y);
            for (int i = 0; i < Length; i++, Adapter.MoveNext())
                Handler(Adapter);
        }

        public static void Contour<T>(this IImageContext Context, IImageContour Contour, double OffsetX, double OffsetY, Action<PixelAdapter<T>> Handler)
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

        public static IEnumerable<QuantizationBox> BoxQuantize<T>(PixelAdapter<T> Adapter, QuantizationType Type, int Count, out Func<QuantizationBox, IReadOnlyPixel, bool> Contain, out Func<QuantizationBox, T> GetColor)
            where T : unmanaged, IPixel
        {
            int Dimension = (Adapter.BitsPerPixel + 7) >> 3;
            int[] Datas = ArrayPool<int>.Shared.Rent(Dimension);
            try
            {
                Action FillDatas;
                Func<QuantizationBox, IEnumerable<QuantizationBox>> Split = Type == QuantizationType.Mean ? Box => Box.MeanSplit() :
                                                                                                            Box => Box.MedianSplit();
                switch (Dimension)
                {
                    case 1:
                        {
                            FillDatas = () => Datas[0] = Adapter.R;
                            Contain = (Box, Pixel) => Box.Contain(Pixel.R);
                            GetColor = Box =>
                            {
                                byte Gray = (byte)Box.GetCenter()[0];
                                return PixelHelper.ToPixel<T>(byte.MaxValue, Gray, Gray, Gray);
                            };
                            break;
                        }
                    case 3:
                        {
                            FillDatas = () =>
                            {
                                Datas[0] = Adapter.R;
                                Datas[1] = Adapter.G;
                                Datas[2] = Adapter.B;
                            };
                            Contain = (Box, Pixel) => Box.Contain(Pixel.R, Pixel.G, Pixel.B);
                            GetColor = Box =>
                            {
                                int[] Center = Box.GetCenter();
                                return PixelHelper.ToPixel<T>(byte.MaxValue, (byte)Center[0], (byte)Center[1], (byte)Center[2]);
                            };
                            break;
                        }
                    case 4:
                    default:
                        {
                            FillDatas = () =>
                            {
                                Datas[0] = Adapter.A;
                                Datas[1] = Adapter.R;
                                Datas[2] = Adapter.G;
                                Datas[3] = Adapter.B;
                            };
                            Contain = (Box, Pixel) => Box.Contain(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                            GetColor = Box =>
                            {
                                int[] Center = Box.GetCenter();
                                return PixelHelper.ToPixel<T>((byte)Center[0], (byte)Center[1], (byte)Center[2], (byte)Center[3]);
                            };
                            break;
                        }
                }

                List<QuantizationBox> Boxes = new List<QuantizationBox> { new QuantizationBox(Dimension, 0, 255) },
                                      UnableSplitBoxes = new List<QuantizationBox>();
                do
                {
                    for (int j = 0; j <= Adapter.MaxY; j++)
                    {
                        Adapter.InternalMove(0, j);
                        for (int i = 0; i <= Adapter.MaxX; i++, Adapter.InternalMoveNext())
                        {
                            FillDatas();
                            foreach (QuantizationBox Box in Boxes)
                                if (Box.InternalTryAddDatas(Datas))
                                    break;
                        }
                    }

                    int ColorCount = Boxes.Count;
                    if (ColorCount >= Count)
                        break;

                    Boxes = Boxes.OrderByDescending(i => i.GetValueSize())
                                 .ToList();

                    int Index = 0;
                    for (; Index < ColorCount && Boxes.Count < Count; Index++)
                    {
                        QuantizationBox[] SplittedBoxes = Split(Boxes[0]).ToArray();
                        Boxes.RemoveAt(0);

                        if (SplittedBoxes.Length == 1)
                        {
                            UnableSplitBoxes.Add(SplittedBoxes[0]);
                            Count--;
                            continue;
                        }

                        Boxes.AddRange(SplittedBoxes);
                    }

                    for (int i = 0; Index < ColorCount; Index++, i++)
                    {
                        QuantizationBox Box = Boxes[0];
                        Count--;
                        Boxes.RemoveAt(0);
                        UnableSplitBoxes.Add(Box);
                    }

                } while (Boxes.Count > 0);

                return Boxes.Concat(UnableSplitBoxes);
            }
            finally
            {
                ArrayPool<int>.Shared.Return(Datas);
            }
        }
        public static IEnumerable<QuantizationBox> BoxQuantize<T>(PixelAdapter<T> Adapter, QuantizationType Type, int Count, ParallelOptions Options, out Func<QuantizationBox, IReadOnlyPixel, bool> Contain, out Func<QuantizationBox, T> GetColor)
            where T : unmanaged, IPixel
        {
            int Dimension = (Adapter.BitsPerPixel + 7) >> 3;

            Action<PixelAdapter<T>, int[]> FillDatas;
            Func<QuantizationBox, IEnumerable<QuantizationBox>> Split = Type == QuantizationType.Mean ? Box => Box.MeanSplit() :
                                                                                                        Box => Box.MedianSplit();
            switch (Dimension)
            {
                case 1:
                    {
                        FillDatas = (Adapter, Datas) => Datas[0] = Adapter.R;
                        Contain = (Box, Pixel) => Box.Contain(Pixel.R);
                        GetColor = Box =>
                        {
                            byte Gray = (byte)Box.GetCenter()[0];
                            return PixelHelper.ToPixel<T>(byte.MaxValue, Gray, Gray, Gray);
                        };
                        break;
                    }
                case 3:
                    {
                        FillDatas = (Adapter, Datas) =>
                        {
                            Datas[0] = Adapter.R;
                            Datas[1] = Adapter.G;
                            Datas[2] = Adapter.B;
                        };
                        Contain = (Box, Pixel) => Box.Contain(Pixel.R, Pixel.G, Pixel.B);
                        GetColor = Box =>
                        {
                            int[] Center = Box.GetCenter();
                            return PixelHelper.ToPixel<T>(byte.MaxValue, (byte)Center[0], (byte)Center[1], (byte)Center[2]);
                        };
                        break;
                    }
                case 4:
                default:
                    {
                        FillDatas = (Adapter, Datas) =>
                        {
                            Datas[0] = Adapter.A;
                            Datas[1] = Adapter.R;
                            Datas[2] = Adapter.G;
                            Datas[3] = Adapter.B;
                        };
                        Contain = (Box, Pixel) => Box.Contain(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                        GetColor = Box =>
                        {
                            int[] Center = Box.GetCenter();
                            return PixelHelper.ToPixel<T>((byte)Center[0], (byte)Center[1], (byte)Center[2], (byte)Center[3]);
                        };
                        break;
                    }
            }

            List<QuantizationBox> Boxes = new List<QuantizationBox> { new QuantizationBox(Dimension, 0, 255) },
                                  UnableSplitBoxes = new List<QuantizationBox>();
            do
            {
                _ = Parallel.For(0, Adapter.MaxY + 1, Options, j =>
                {
                    PixelAdapter<T> Adapter2 = Adapter.Clone();
                    Adapter2.InternalMove(0, j);
                    int[] Datas = ArrayPool<int>.Shared.Rent(Dimension);
                    try
                    {
                        for (int i = 0; i <= Adapter.MaxX; i++, Adapter2.InternalMoveNext())
                        {
                            FillDatas(Adapter2, Datas);
                            foreach (QuantizationBox Box in Boxes)
                                if (Box.InternalTryAddDatas(Datas))
                                    break;
                        }
                    }
                    finally
                    {
                        ArrayPool<int>.Shared.Return(Datas);
                    }
                });

                int ColorCount = Boxes.Count;
                if (ColorCount >= Count)
                    break;

                Boxes = Boxes.OrderByDescending(i => i.GetValueSize())
                             .ToList();

                int Index = 0;
                for (; Index < ColorCount && Boxes.Count < Count; Index++)
                {
                    QuantizationBox[] SplittedBoxes = Split(Boxes[0]).ToArray();
                    Boxes.RemoveAt(0);

                    if (SplittedBoxes.Length == 1)
                    {
                        UnableSplitBoxes.Add(SplittedBoxes[0]);
                        Count--;
                        continue;
                    }

                    Boxes.AddRange(SplittedBoxes);
                }

                for (int i = 0; Index < ColorCount; Index++, i++)
                {
                    QuantizationBox Box = Boxes[0];
                    Count--;
                    Boxes.RemoveAt(0);
                    UnableSplitBoxes.Add(Box);
                }

            } while (Boxes.Count > 0);

            return Boxes.Concat(UnableSplitBoxes);

        }

    }
}