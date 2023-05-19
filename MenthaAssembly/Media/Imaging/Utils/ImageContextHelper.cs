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
        public static void DrawLine<T>(PixelAdapter<T> Context, int X0, int Y0, int X1, int Y1, PixelAdapter<T> Pen, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            int X = X0 - (Pen.XLength >> 1),
                Y = Y0 - (Pen.YLength >> 1),
                DeltaX = X1 - X0,
                DeltaY = Y1 - Y0;

            GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, Math.Abs(DeltaX), Math.Abs(DeltaY),
                                                    (Dx, Dy) => DrawStamp(Context, X + Dx, Y + Dy, Pen, Blend));
        }
        public static void DrawLine<T>(PixelAdapter<T> Context, int X0, int Y0, int X1, int Y1, Action<PixelAdapter<T>> Handler)
            where T : unmanaged, IPixel
        {
            if (X1 < X0)
            {
                MathHelper.Swap(ref X0, ref X1);
                MathHelper.Swap(ref Y0, ref Y1);
            }
            int DeltaX = X1 - X0,
                DeltaY = Y1 - Y0,
                AbsDeltaY = Math.Abs(DeltaY);

            Dictionary<int, int> LeftBound = new(),
                                 RightBound = new();
            #region Line Body Bound
            int Iw = Context.XLength,
                Ih = Context.YLength,
                MaxX = Iw - 1,
                RTx,
                RTy;

            GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, DeltaX, AbsDeltaY, (Dx, Dy) =>
            {
                RTy = Y0 + Dy;
                if (-1 < RTy && RTy < Ih)
                {
                    RTx = Math.Min(Math.Max(X0 + Dx, 0), MaxX);

                    // Left
                    if (LeftBound.TryGetValue(RTy, out int LastRx))
                    {
                        if (LastRx > RTx)
                            LeftBound[RTy] = RTx;
                    }
                    else
                    {
                        LeftBound[RTy] = RTx;
                    }

                    // Right
                    if (RightBound.TryGetValue(RTy, out LastRx))
                    {
                        if (LastRx < RTx)
                            RightBound[RTy] = RTx;
                    }
                    else
                    {
                        RightBound[RTy] = RTx;
                    }
                }
            });

            #endregion
            #region Fill
            foreach (KeyValuePair<int, int> Data in RightBound)
            {
                int Y = Data.Key,
                    TRx = Data.Value;
                if (LeftBound.TryGetValue(Y, out int TLx))
                {
                    LeftBound.Remove(Y);

                    Context.DangerousMove(TLx, Y);
                    for (; TLx <= TRx; TLx++, Context.DangerousMoveNextX())
                        Handler(Context);
                }
                else
                {
                    Context.DangerousMove(TRx, Y);
                    Handler(Context);
                }
            }
            RightBound.Clear();

            foreach (KeyValuePair<int, int> Data in LeftBound)
            {
                Context.DangerousMove(Data.Value, Data.Key);
                Handler(Context);
            }

            LeftBound.Clear();

            #endregion
        }
        public static void DrawLine<T>(PixelAdapter<T> Context, int X0, int Y0, int X1, int Y1, ImageContour Contour, int ContourCx, int ContourCy, Action<PixelAdapter<T>> Handler)
            where T : unmanaged, IPixel
        {
            int Iw = Context.XLength,
                Ih = Context.YLength;

            if (X1 < X0)
            {
                MathHelper.Swap(ref X0, ref X1);
                MathHelper.Swap(ref Y0, ref Y1);
            }
            int DeltaX = X1 - X0,
                DeltaY = Y1 - Y0,
                AbsDeltaY = Math.Abs(DeltaY);

            bool IsHollow = false;
            Dictionary<int, int> LeftBound = new(),
                                 RightBound = new();
            #region Pen Bound
            int MaxX = Iw - 1,
                DUx = 0,
                DUy = 0,
                DLx = 0,
                DLy = 0,
                UpperDistance = 0,
                LowerDistance = 0;

            foreach (KeyValuePair<int, ImageContourScanLine> Item in Contour)
            {
                int j = Item.Key;
                ImageContourScanLine Data = Item.Value;

                int DataLength = Data.Length;
                if (DataLength > 2)
                {
                    IsHollow = true;
                    LeftBound.Clear();
                    RightBound.Clear();
                    break;
                }

                int Ty = j - ContourCy;
                // Found Left Bound
                {
                    int Tx = Data[0] - ContourCx,
                        Predict = DeltaX * Ty - DeltaY * Tx,
                        Distance = Math.Abs(Predict);

                    // UpperLine
                    if (Predict > 0)
                    {
                        if (UpperDistance < Distance)
                        {
                            UpperDistance = Distance;
                            DUx = Tx;
                            DUy = Ty;
                        }
                    }

                    // LowerLine
                    else
                    {
                        if (LowerDistance < Distance)
                        {
                            LowerDistance = Distance;
                            DLx = Tx;
                            DLy = Ty;
                        }
                    }

                    // StartPoint
                    int Rx = Math.Min(Math.Max(Tx + X0, 0), MaxX),
                        Ry = Ty + Y0;
                    if (-1 < Ry && Ry < Ih &&
                        (!LeftBound.TryGetValue(Ry, out int LastRx) || LastRx > Rx))
                        LeftBound[Ry] = Rx;

                    // EndPoint
                    Rx = Math.Min(Math.Max(Tx + X1, 0), MaxX);
                    Ry = Ty + Y1;
                    if (-1 < Ry && Ry < Ih &&
                        (!LeftBound.TryGetValue(Ry, out LastRx) || LastRx > Rx))
                        LeftBound[Ry] = Rx;
                }

                // Found Right Bound
                {
                    int Tx = Data[DataLength - 1] - ContourCx,
                        Predict = DeltaX * Ty - DeltaY * Tx,
                        Distance = Math.Abs(Predict);

                    // UpperLine
                    if (Predict > 0)
                    {
                        if (UpperDistance < Distance)
                        {
                            UpperDistance = Distance;
                            DUx = Tx;
                            DUy = Ty;
                        }
                    }

                    // LowerLine
                    else
                    {
                        if (LowerDistance < Distance)
                        {
                            LowerDistance = Distance;
                            DLx = Tx;
                            DLy = Ty;
                        }
                    }

                    // StartPoint
                    int Rx = Math.Min(Math.Max(Tx + X0, 0), MaxX),
                        Ry = Ty + Y0;

                    if (-1 < Ry && Ry < Ih &&
                        (!RightBound.TryGetValue(Ry, out int LastRx) || LastRx < Rx))
                        RightBound[Ry] = Rx;

                    // EndPoint
                    Rx = Math.Min(Math.Max(Tx + X1, 0), MaxX);
                    Ry = Ty + Y1;

                    if (-1 < Ry && Ry < Ih &&
                        (!RightBound.TryGetValue(Ry, out LastRx) || LastRx < Rx))
                        RightBound[Ry] = Rx;
                }
            }

            #endregion

            if (IsHollow)
            {
                #region Line Body Bound
                ImageContour LineContour = new(),
                             Stroke = ImageContour.Offset(Contour, X0 - ContourCx, Y0 - ContourCy);

                int LastDx = 0,
                    LastDy = 0;

                GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, DeltaX, AbsDeltaY, (Dx, Dy) =>
                {
                    Stroke.Offset(Dx - LastDx, Dy - LastDy);
                    LineContour.Union(Stroke);

                    LastDx = Dx;
                    LastDy = Dy;
                });
                #endregion

                ImageContextHelper.FillContour(Context, Contour, 0d, 0d, Handler);
            }
            else
            {
                #region Line Body Bound
                int Ux = X0 + DUx,
                    Uy = Y0 + DUy,
                    Lx = X0 + DLx,
                    Ly = Y0 + DLy,
                    RTx, RTy;

                if (DeltaX == 0 && DeltaY < 0)
                {
                    MathHelper.Swap(ref Ux, ref Lx);
                    MathHelper.Swap(ref Uy, ref Ly);
                }

                GraphicDeltaHandler FoundLineBodyBound = DeltaX * DeltaY < 0 ?
                    new GraphicDeltaHandler(
                        (Dx, Dy) =>
                        {
                            // Right
                            RTx = Math.Min(Math.Max(Ux + Dx, 0), MaxX);
                            RTy = Uy + Dy;
                            if (-1 < RTy && RTy < Ih &&
                                (!RightBound.TryGetValue(RTy, out int LastRx) || LastRx < RTx))
                                RightBound[RTy] = RTx;

                            // Left
                            RTx = Math.Min(Math.Max(Lx + Dx, 0), MaxX);
                            RTy = Ly + Dy;
                            if (-1 < RTy && RTy < Ih &&
                                (!LeftBound.TryGetValue(RTy, out LastRx) || LastRx > RTx))
                                LeftBound[RTy] = RTx;
                        }) :
                        (Dx, Dy) =>
                        {
                            // Left
                            RTx = Math.Min(Math.Max(Ux + Dx, 0), MaxX);
                            RTy = Uy + Dy;
                            if (-1 < RTy && RTy < Ih &&
                                (!LeftBound.TryGetValue(RTy, out int LastRx) || LastRx > RTx))
                                LeftBound[RTy] = RTx;

                            // Right
                            RTx = Math.Min(Math.Max(Lx + Dx, 0), MaxX);
                            RTy = Ly + Dy;
                            if (-1 < RTy && RTy < Ih &&
                                (!RightBound.TryGetValue(RTy, out LastRx) || LastRx < RTx))
                                RightBound[RTy] = RTx;
                        };

                GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, DeltaX, AbsDeltaY, FoundLineBodyBound);

                #endregion
                #region Fill
                foreach (KeyValuePair<int, int> Data in RightBound)
                {
                    int Y = Data.Key,
                        TRx = Data.Value;
                    if (LeftBound.TryGetValue(Y, out int TLx))
                    {
                        LeftBound.Remove(Y);

                        Context.DangerousMove(TLx, Y);
                        for (; TLx <= TRx; TLx++, Context.DangerousMoveNextX())
                            Handler(Context);
                    }
                    else
                    {
                        Context.DangerousMove(TRx, Y);
                        Handler(Context);
                    }
                }
                RightBound.Clear();

                foreach (KeyValuePair<int, int> Data in LeftBound)
                {
                    Context.DangerousMove(Data.Value, Data.Key);
                    Handler(Context);
                }

                LeftBound.Clear();

                #endregion
            }
        }

        public static void DrawStamp<T>(PixelAdapter<T> Context, int X, int Y, PixelAdapter<T> Stamp, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            int Pw = Stamp.XLength,
                Ph = Stamp.YLength,
                Sx = X - (Pw >> 1),
                Sy = Y - (Ph >> 1),
                Ex = Sx + Pw,
                Ey = Sy + Ph,
                SourceX = 0,
                SourceY = 0;

            if (Sx < 0)
            {
                SourceX -= Sx;
                Sx = 0;
            }

            if (Sy < 0)
            {
                SourceY -= Sy;
                Sy = 0;
            }

            Pw = Math.Min(Ex, Context.XLength - 1) - Sx;
            if (Pw < 1)
                return;

            Ph = Math.Min(Ey, Context.YLength - 1) - Sy;
            if (Ph < 1)
                return;

            Stamp.DangerousMove(SourceX, SourceY);
            Context.DangerousMove(Sx, Sy);
            for (int j = 0; j < Ph; j++, Stamp.DangerousMoveNextY(), Context.DangerousMoveNextY())
            {
                for (int i = 0; i < Pw; i++, Stamp.DangerousMoveNextX(), Context.DangerousMoveNextX())
                {
                    if (Blend == BlendMode.Overlay)
                    {
                        byte Alpha = Stamp.A;
                        if (Alpha == byte.MinValue)
                            continue;

                        if (Alpha == byte.MaxValue)
                            Context.Override(Stamp);
                        else
                            Context.Overlay(Stamp);
                    }
                    else
                    {
                        Context.Override(Stamp);
                    }
                }

                Stamp.DangerousOffsetX(-Pw);
                Context.DangerousOffsetX(-Pw);
            }
        }

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