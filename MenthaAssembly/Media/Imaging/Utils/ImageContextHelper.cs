using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public delegate bool ImagePredicate(int X, int Y, IReadOnlyPixel Pixel);

    internal static unsafe class ImageContextHelper
    {
        public static void ScanLine<T>(this IImageContext Context, int X, int Y, int Length, Action<PixelAdapter<T>> Handler)
            where T : unmanaged, IPixel
        {
            PixelAdapter<T> Adapter = Context.GetAdapter<T>(X, Y);
            for (int i = 0; i < Length; i++, Adapter.MoveNextX())
                Handler(Adapter);
        }

        public static void Contour<T>(this IImageContext Context, IImageContour Contour, double OffsetX, double OffsetY, Action<PixelAdapter<T>> Handler)
            where T : unmanaged, IPixel
        {
            Contour.EnsureContents();
            int Dx = (int)Math.Round(Contour.OffsetX + OffsetX),
                Dy = (int)Math.Round(Contour.OffsetY + OffsetY);

            PixelAdapter<T> Adapter = Context.GetAdapter<T>(0, 0);
            int MaxX = Adapter.XLength - 1,
                MaxY = Adapter.YLength - 1,
                Ty;
            foreach (KeyValuePair<int, ImageContourScanLine> Content in Contour.Contents)
            {
                Ty = Content.Key + Dy;
                if (Ty < 0 || MaxY < Ty)
                    continue;

                Adapter.DangerousMove(0, Ty);

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

                    Adapter.DangerousOffsetX(Sx - CurrentX);
                    for (int j = Sx; j <= Ex; j++, Adapter.DangerousMoveNextX())
                        Handler(Adapter);

                    CurrentX = Ex + 1;
                }
            }
        }

        public static ImageContour FindBound(this IImageContext Context, int SeedX, int SeedY, ImagePredicate Predicate)
        {
            int Width = Context.Width,
                Height = Context.Height;

            if (SeedX < 0 || Width <= SeedX ||
                SeedY < 0 || Height <= SeedY)
                return null;

            ImageContour Contour = new();
            Stack<int> StackX = new(),
                       StackY = new();
            StackX.Push(SeedX);
            StackY.Push(SeedY);

            int X, Y, SaveX, Rx, Lx;

            IPixelAdapter Seed, Pixel;
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
                    Pixel.MoveNextX();
                }

                // Find Left Bound
                Rx = X - 1;
                X = SaveX - 1;

                Pixel = Seed.Clone();
                Pixel.MovePreviousX();
                while (-1 < X && !Predicate(X, Y, Pixel))
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

                Seed = Context.GetAdapter(X, Y);
                if (-1 < Y && Y < Height &&
                    !Contour.Contain(X, Y))
                    for (; X <= Rx; X++)
                    {
                        while (X <= Rx && !Predicate(X, Y, Seed))
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

            return Contour;
        }

        public static IEnumerable<QuantizationBox> BoxQuantize<T>(PixelAdapter<T> Adapter, QuantizationTypes Type, int Count, out Func<QuantizationBox, PixelAdapter<T>, bool> Contain, out Func<QuantizationBox, T> GetColor)
            where T : unmanaged, IPixel
        {
            // Initialize Palette
            Dictionary<int, int> Palette = new();
            for (int j = 0; j < Adapter.YLength; j++)
            {
                Adapter.DangerousMove(0, j);
                for (int i = 0; i < Adapter.XLength; i++, Adapter.DangerousMoveNextX())
                {
                    int Key = Adapter.A << 24 | Adapter.R << 16 | Adapter.G << 8 | Adapter.B;
                    if (Palette.ContainsKey(Key))
                        Palette[Key]++;
                    else
                        Palette[Key] = 1;
                }
            }

            int Dimension = (Adapter.BitsPerPixel + 7) >> 3;
            int[] Datas = ArrayPool<int>.Shared.Rent(Dimension);
            try
            {
                Action<int> FillDatas;
                Func<QuantizationBox, IEnumerable<QuantizationBox>> Split = Type == QuantizationTypes.Mean ? Box => Box.MeanSplit() :
                                                                                                             Box => Box.MedianSplit();
                switch (Dimension)
                {
                    case 1:
                        {
                            FillDatas = Key => Datas[0] = (Key >> 16) & 0xFF;
                            Contain = (Box, Pixel) => Box.Contain(Pixel.R);
                            GetColor = Box =>
                            {
                                byte Gray = (byte)Box.GetValueCenter()[0];
                                return PixelHelper.ToPixel<T>(byte.MaxValue, Gray, Gray, Gray);
                            };
                            break;
                        }
                    case 3:
                        {
                            FillDatas = Key =>
                            {
                                Datas[0] = (Key >> 16) & 0xFF;
                                Datas[1] = (Key >> 8) & 0xFF;
                                Datas[2] = Key & 0xFF;
                            };
                            Contain = (Box, Pixel) => Box.Contain(Pixel.R, Pixel.G, Pixel.B);
                            GetColor = Box =>
                            {
                                int[] Center = Box.GetValueCenter();
                                return PixelHelper.ToPixel<T>(byte.MaxValue, (byte)Center[0], (byte)Center[1], (byte)Center[2]);
                            };
                            break;
                        }
                    case 4:
                    default:
                        {
                            FillDatas = Key =>
                            {
                                Datas[0] = (Key >> 24) & 0xFF;
                                Datas[1] = (Key >> 16) & 0xFF;
                                Datas[2] = (Key >> 8) & 0xFF;
                                Datas[3] = Key & 0xFF;
                            };
                            Contain = (Box, Pixel) => Box.Contain(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                            GetColor = Box =>
                            {
                                int[] Center = Box.GetValueCenter();
                                return PixelHelper.ToPixel<T>((byte)Center[0], (byte)Center[1], (byte)Center[2], (byte)Center[3]);
                            };
                            break;
                        }
                }

                List<QuantizationBox> Boxes = new() { new QuantizationBox(Dimension, 0, 255) },
                                      UnableSplitBoxes = new();
                do
                {
                    foreach (int Key in Palette.Keys)
                    {
                        FillDatas(Key);
                        int DataCount = Palette[Key];

                        foreach (QuantizationBox Box in Boxes)
                            if (Box.InternalTryAddDatas(Datas, DataCount))
                                break;
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
                        UnableSplitBoxes.Add(Boxes[0]);
                        Boxes.RemoveAt(0);
                        Count--;
                    }

                } while (Boxes.Count > 0);

                return Boxes.Concat(UnableSplitBoxes);
            }
            finally
            {
                ArrayPool<int>.Shared.Return(Datas);
                Palette.Clear();
            }
        }
        public static IEnumerable<QuantizationBox> BoxQuantize<T>(PixelAdapter<T> Adapter, QuantizationTypes Type, int Count, ParallelOptions Options, out Func<QuantizationBox, PixelAdapter<T>, bool> Contain, out Func<QuantizationBox, T> GetColor)
            where T : unmanaged, IPixel
        {
            // Initialize Palette
            Dictionary<int, int> Palette = new();
            for (int j = 0; j < Adapter.YLength; j++)
            {
                Adapter.DangerousMove(0, j);
                for (int i = 0; i < Adapter.XLength; i++, Adapter.DangerousMoveNextX())
                {
                    int Key = Adapter.A << 24 | Adapter.R << 16 | Adapter.G << 8 | Adapter.B;
                    if (Palette.ContainsKey(Key))
                        Palette[Key]++;
                    else
                        Palette[Key] = 1;
                }
            }

            int Dimension = (Adapter.BitsPerPixel + 7) >> 3;
            Action<int[], int> FillDatas;
            Func<QuantizationBox, IEnumerable<QuantizationBox>> Split = Type == QuantizationTypes.Mean ? Box => Box.MeanSplit() :
                                                                                                         Box => Box.MedianSplit();
            switch (Dimension)
            {
                case 1:
                    {
                        FillDatas = (Datas, Key) => Datas[0] = (Key >> 16) & 0xFF;
                        Contain = (Box, Pixel) => Box.Contain(Pixel.R);
                        GetColor = Box =>
                        {
                            byte Gray = (byte)Box.GetValueCenter()[0];
                            return PixelHelper.ToPixel<T>(byte.MaxValue, Gray, Gray, Gray);
                        };
                        break;
                    }
                case 3:
                    {
                        FillDatas = (Datas, Key) =>
                        {
                            Datas[0] = (Key >> 16) & 0xFF;
                            Datas[1] = (Key >> 8) & 0xFF;
                            Datas[2] = Key & 0xFF;
                        };
                        Contain = (Box, Pixel) => Box.Contain(Pixel.R, Pixel.G, Pixel.B);
                        GetColor = Box =>
                        {
                            int[] Center = Box.GetValueCenter();
                            return PixelHelper.ToPixel<T>(byte.MaxValue, (byte)Center[0], (byte)Center[1], (byte)Center[2]);
                        };
                        break;
                    }
                case 4:
                default:
                    {
                        FillDatas = (Datas, Key) =>
                        {
                            Datas[0] = (Key >> 24) & 0xFF;
                            Datas[1] = (Key >> 16) & 0xFF;
                            Datas[2] = (Key >> 8) & 0xFF;
                            Datas[3] = Key & 0xFF;
                        };
                        Contain = (Box, Pixel) => Box.Contain(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                        GetColor = Box =>
                        {
                            int[] Center = Box.GetValueCenter();
                            return PixelHelper.ToPixel<T>((byte)Center[0], (byte)Center[1], (byte)Center[2], (byte)Center[3]);
                        };
                        break;
                    }
            }

            List<QuantizationBox> Boxes = new() { new QuantizationBox(Dimension, 0, 255) },
                                  UnableSplitBoxes = new();
            do
            {
                _ = Parallel.ForEach(Palette, Options, Content =>
                {
                    int[] Datas = ArrayPool<int>.Shared.Rent(Dimension);
                    try
                    {
                        FillDatas(Datas, Content.Key);
                        foreach (QuantizationBox Box in Boxes)
                            if (Box.InternalTryAddDatas(Datas, Content.Value))
                                break;
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
                    UnableSplitBoxes.Add(Boxes[0]);
                    Boxes.RemoveAt(0);
                    Count--;
                }

            } while (Boxes.Count > 0);

            return Boxes.Concat(UnableSplitBoxes);
        }

        public static IEnumerable<QuantizationCluster> ClusterQuantize<T>(PixelAdapter<T> Adapter, int Count, out Func<QuantizationCluster, PixelAdapter<T>, int> GetDistanceConst, out Func<QuantizationCluster, T> GetColor)
            where T : unmanaged, IPixel
        {
            List<QuantizationCluster> Clusters = new(Count);
            int Dimension = (Adapter.BitsPerPixel + 7) >> 3;

            Action<int[], int> FillDatas;
            switch (Dimension)
            {
                case 1:
                    {
                        FillDatas = (Datas, Key) => Datas[0] = (Key >> 16) & 0xFF;
                        GetDistanceConst = (Cluster, Pixel) => Cluster.Center[0] - Pixel.R;
                        GetColor = Cluster =>
                        {
                            byte Gray = (byte)Cluster.Center[0];
                            return PixelHelper.ToPixel<T>(byte.MaxValue, Gray, Gray, Gray);
                        };
                        break;
                    }
                case 3:
                    {
                        FillDatas = (Datas, Key) =>
                        {
                            Datas[0] = (Key >> 16) & 0xFF;
                            Datas[1] = (Key >> 8) & 0xFF;
                            Datas[2] = Key & 0xFF;
                        };
                        GetDistanceConst = (Cluster, Pixel) =>
                        {
                            int[] Center = Cluster.Center;
                            int Delta = Center[0] - Pixel.R,
                                Sum = Delta * Delta;

                            Delta = Center[1] - Pixel.G;
                            Sum += Delta * Delta;

                            Delta = Center[2] - Pixel.B;
                            Sum += Delta * Delta;

                            return Sum;
                        };
                        GetColor = Cluster =>
                        {
                            int[] Center = Cluster.Center;
                            return PixelHelper.ToPixel<T>(byte.MaxValue, (byte)Center[0], (byte)Center[1], (byte)Center[2]);
                        };
                        break;
                    }
                case 4:
                default:
                    {
                        FillDatas = (Datas, Key) =>
                        {
                            Datas[0] = (Key >> 24) & 0xFF;
                            Datas[1] = (Key >> 16) & 0xFF;
                            Datas[2] = (Key >> 8) & 0xFF;
                            Datas[3] = Key & 0xFF;
                        };
                        GetDistanceConst = (Cluster, Pixel) =>
                        {
                            int[] Center = Cluster.Center;
                            int Delta = Center[0] - Pixel.A,
                                Sum = Delta * Delta;

                            Delta = Center[1] - Pixel.R;
                            Sum += Delta * Delta;

                            Delta = Center[2] - Pixel.G;
                            Sum += Delta * Delta;

                            Delta = Center[3] - Pixel.B;
                            Sum += Delta * Delta;

                            return Sum;
                        };
                        GetColor = Cluster =>
                        {
                            int[] Center = Cluster.Center;
                            return PixelHelper.ToPixel<T>((byte)Center[0], (byte)Center[1], (byte)Center[2], (byte)Center[3]);
                        };
                        break;
                    }
            }

            int GetDatasDistanceConst(int[] Center1, int[] Center2)
            {
                int Sum = 0;
                for (int i = 0; i < Dimension; i++)
                {
                    int Delta = Center2[i] - Center1[i];
                    Sum += Delta * Delta;
                }
                return Sum;
            }

            // Initializes Clusters
            Dictionary<int, int> Palette = new();
            int[] Datas = new int[Dimension],
                  Empty = new int[Dimension],
                  MinCenter = null,
                  MaxCenter = null;
            int MinD = int.MaxValue,
                MaxD = int.MinValue,
                D;
            for (int j = 0; j < Adapter.YLength; j++)
            {
                Adapter.DangerousMove(0, j);
                for (int i = 0; i < Adapter.XLength; i++, Adapter.DangerousMoveNextX())
                {
                    int Key = Adapter.A << 24 | Adapter.R << 16 | Adapter.G << 8 | Adapter.B;
                    if (Palette.ContainsKey(Key))
                    {
                        Palette[Key]++;
                        continue;
                    }

                    FillDatas(Datas, Key);

                    D = GetDatasDistanceConst(Empty, Datas);
                    if (D < MinD)
                    {
                        MinD = D;
                        MinCenter = Datas;
                        Datas = new int[Dimension];
                    }
                    else if (MaxD < D)
                    {
                        MaxD = D;
                        MaxCenter = Datas;
                        Datas = new int[Dimension];
                    }

                    Palette[Key] = 1;
                }
            }

            Clusters.Add(new QuantizationCluster(MinCenter));
            if (MaxCenter != null)
            {
                Clusters.Add(new QuantizationCluster(MaxCenter));

                const float DenominatorF = 65025f;
                float MaxF, F;
                Datas = new int[Dimension];
                for (int k = Clusters.Count; k < Count; k++)
                {
                    MaxF = 0f;
                    MaxCenter = null;
                    foreach (int Key in Palette.Keys)
                    {
                        FillDatas(Datas, Key);

                        D = GetDatasDistanceConst(MinCenter, Datas);
                        if (D == 0)
                            continue;

                        F = D / DenominatorF;
                        for (int w = 1; w < Clusters.Count; w++)
                        {
                            D = GetDatasDistanceConst(Clusters[w].Center, Datas);
                            if (D == 0)
                            {
                                F = 0f;
                                break;
                            }

                            F *= D / DenominatorF;
                        }

                        if (MaxF < F)
                        {
                            MaxF = F;
                            MaxCenter = Datas;
                            Datas = new int[Dimension];
                        }
                    }

                    if (MaxCenter is null)
                        break;

                    Clusters.Add(new QuantizationCluster(MaxCenter));
                }
            }

            // Initializes Distances
            Count = Clusters.Count;
            Datas = Clusters[0].Center;
            int[] Distances = new int[Count - 1];
            for (int i = 0; i < Distances.Length; i++)
                Distances[i] = GetDatasDistanceConst(Datas, Clusters[i + 1].Center);

            // Finds Clusters
            try
            {
                Datas = ArrayPool<int>.Shared.Rent(Dimension);
                bool IsEnd;
                do
                {
                    // Adds Datas
                    int Index;
                    foreach (int Key in Palette.Keys)
                    {
                        FillDatas(Datas, Key);

                        // Finds Minimum Distance
                        Index = -1;
                        MinD = int.MaxValue;
                        for (int k = 0; k < Clusters.Count; k++)
                        {
                            D = GetDatasDistanceConst(Datas, Clusters[k].Center);
                            if (D < MinD)
                            {
                                MinD = D;
                                Index = k;
                            }
                        }

                        Clusters[Index].InternalAddDatas(Datas, Palette[Key]);
                    }

                    // Checks the distance of clusters.
                    IsEnd = true;

                    int[] Cluster1 = Clusters[0].GetNextCenter();
                    Clusters[0].Center = Cluster1;
                    for (int i = 0; i < Distances.Length; i++)
                    {
                        Empty = Clusters[i + 1].GetNextCenter();
                        D = GetDatasDistanceConst(Cluster1, Empty);
                        if (IsEnd && Distances[i] != D)
                            IsEnd = false;

                        Distances[i] = D;
                        Clusters[i + 1].Center = Empty;
                    }

                } while (!IsEnd);

                return Clusters;
            }
            finally
            {
                ArrayPool<int>.Shared.Return(Datas);
            }
        }
        public static IEnumerable<QuantizationCluster> ClusterQuantize<T>(PixelAdapter<T> Adapter, int Count, ParallelOptions Options, out Func<QuantizationCluster, PixelAdapter<T>, int> GetDistanceConst, out Func<QuantizationCluster, T> GetColor)
            where T : unmanaged, IPixel
        {
            List<QuantizationCluster> Clusters = new(Count);
            int Dimension = (Adapter.BitsPerPixel + 7) >> 3;

            Action<int[], int> FillDatas;
            Func<int[], int> DataToKey;
            switch (Dimension)
            {
                case 1:
                    {
                        FillDatas = (Datas, Key) => Datas[0] = (Key >> 16) & 0xFF;
                        DataToKey = Datas => 255 << 24 | Datas[0] << 16 | Datas[0] << 8 | Datas[0];
                        GetDistanceConst = (Cluster, Pixel) => Cluster.Center[0] - Pixel.R;
                        GetColor = Cluster =>
                        {
                            byte Gray = (byte)Cluster.Center[0];
                            return PixelHelper.ToPixel<T>(byte.MaxValue, Gray, Gray, Gray);
                        };
                        break;
                    }
                case 3:
                    {
                        FillDatas = (Datas, Key) =>
                        {
                            Datas[0] = (Key >> 16) & 0xFF;
                            Datas[1] = (Key >> 8) & 0xFF;
                            Datas[2] = Key & 0xFF;
                        };
                        DataToKey = Datas => 255 << 24 | Datas[0] << 16 | Datas[1] << 8 | Datas[2];
                        GetDistanceConst = (Cluster, Pixel) =>
                        {
                            int[] Center = Cluster.Center;
                            int Delta = Center[0] - Pixel.R,
                                Sum = Delta * Delta;

                            Delta = Center[1] - Pixel.G;
                            Sum += Delta * Delta;

                            Delta = Center[2] - Pixel.B;
                            Sum += Delta * Delta;

                            return Sum;
                        };
                        GetColor = Cluster =>
                        {
                            int[] Center = Cluster.Center;
                            return PixelHelper.ToPixel<T>(byte.MaxValue, (byte)Center[0], (byte)Center[1], (byte)Center[2]);
                        };
                        break;
                    }
                case 4:
                default:
                    {
                        FillDatas = (Datas, Key) =>
                        {
                            Datas[0] = (Key >> 24) & 0xFF;
                            Datas[1] = (Key >> 16) & 0xFF;
                            Datas[2] = (Key >> 8) & 0xFF;
                            Datas[3] = Key & 0xFF;
                        };
                        DataToKey = Datas => Datas[0] << 24 | Datas[1] << 16 | Datas[2] << 8 | Datas[3];
                        GetDistanceConst = (Cluster, Pixel) =>
                        {
                            int[] Center = Cluster.Center;
                            int Delta = Center[0] - Pixel.A,
                                Sum = Delta * Delta;

                            Delta = Center[1] - Pixel.R;
                            Sum += Delta * Delta;

                            Delta = Center[2] - Pixel.G;
                            Sum += Delta * Delta;

                            Delta = Center[3] - Pixel.B;
                            Sum += Delta * Delta;

                            return Sum;
                        };
                        GetColor = Cluster =>
                        {
                            int[] Center = Cluster.Center;
                            return PixelHelper.ToPixel<T>((byte)Center[0], (byte)Center[1], (byte)Center[2], (byte)Center[3]);
                        };
                        break;
                    }
            }

            int GetDatasDistanceConst(int[] Center1, int[] Center2)
            {
                int Sum = 0;
                for (int i = 0; i < Dimension; i++)
                {
                    int Delta = Center2[i] - Center1[i];
                    Sum += Delta * Delta;
                }
                return Sum;
            }

            // Initializes Clusters
            Dictionary<int, int> Palette = new();
            int[] MinCenter = null,
                  MaxCenter = null;
            {
                int[] Datas = new int[Dimension],
                      Empty = new int[Dimension];
                int MinD = int.MaxValue,
                    MaxD = int.MinValue,
                    D;
                for (int j = 0; j < Adapter.YLength; j++)
                {
                    Adapter.DangerousMove(0, j);
                    for (int i = 0; i < Adapter.XLength; i++, Adapter.DangerousMoveNextX())
                    {
                        int Key = Adapter.A << 24 | Adapter.R << 16 | Adapter.G << 8 | Adapter.B;
                        if (Palette.ContainsKey(Key))
                        {
                            Palette[Key]++;
                            continue;
                        }

                        FillDatas(Datas, Key);

                        D = GetDatasDistanceConst(Empty, Datas);
                        if (D < MinD)
                        {
                            MinD = D;
                            MinCenter = Datas;
                            Datas = new int[Dimension];
                        }
                        else if (MaxD < D)
                        {
                            MaxD = D;
                            MaxCenter = Datas;
                            Datas = new int[Dimension];
                        }

                        Palette[Key] = 1;
                    }
                }
            }

            Clusters.Add(new QuantizationCluster(MinCenter));
            if (MaxCenter != null)
            {
                Clusters.Add(new QuantizationCluster(MaxCenter));

                const float DenominatorF = 65025f;
                float MaxF;
                for (int k = Clusters.Count; k < Count; k++)
                {
                    MaxF = 0f;
                    MaxCenter = null;
                    _ = Parallel.ForEach(Palette.Keys, Options, Key =>
                    {
                        int[] Buffer = new int[Dimension];
                        FillDatas(Buffer, Key);

                        int D;
                        float F = 1f;
                        for (int w = 0; w < Clusters.Count; w++)
                        {
                            D = GetDatasDistanceConst(Clusters[w].Center, Buffer);
                            if (D == 0)
                                return;

                            F *= D / DenominatorF;
                        }

                        if (MaxF < F)
                        {
                            MaxF = F;
                            MaxCenter = Buffer;
                        }
                    });

                    if (MaxCenter is null)
                        break;

                    Clusters.Add(new QuantizationCluster(MaxCenter));
                }
            }

            // Initializes Distances
            Count = Clusters.Count;
            int[] Distances = new int[Count - 1];
            {
                int[] Datas = Clusters[0].Center;
                _ = Parallel.For(0, Distances.Length, Options,
                    i => Distances[i] = GetDatasDistanceConst(Datas, Clusters[i + 1].Center));
            }

            // Finds Clusters
            bool IsEnd;
            do
            {
                // Adds Datas
                _ = Parallel.ForEach(Palette.Keys, Options, Key =>
                {
                    int[] Buffer = ArrayPool<int>.Shared.Rent(Dimension);
                    try
                    {
                        FillDatas(Buffer, Key);

                        // Finds Minimum Distance
                        int Index = -1,
                            MinD = int.MaxValue,
                            D;
                        for (int k = 0; k < Clusters.Count; k++)
                        {
                            D = GetDatasDistanceConst(Buffer, Clusters[k].Center);
                            if (D < MinD)
                            {
                                MinD = D;
                                Index = k;
                            }
                        }

                        Clusters[Index].InternalAddDatas(Buffer, Palette[Key]);
                    }
                    finally
                    {
                        ArrayPool<int>.Shared.Return(Buffer);
                    }
                });

                IsEnd = true;

                // Checks the distance of clusters.
                int[] Cluster1 = Clusters[0].GetNextCenter();
                Clusters[0].Center = Cluster1;
                _ = Parallel.For(0, Distances.Length, Options, i =>
                {
                    int[] Cluster2 = Clusters[i + 1].GetNextCenter();
                    int D = GetDatasDistanceConst(Cluster1, Cluster2);
                    if (IsEnd && Distances[i] != D)
                        IsEnd = false;

                    Distances[i] = D;
                    Clusters[i + 1].Center = Cluster2;
                });

            } while (!IsEnd);

            return Clusters;
        }


    }
}